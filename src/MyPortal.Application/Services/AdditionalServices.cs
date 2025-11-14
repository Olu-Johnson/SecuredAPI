using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Application.Services;

public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public GroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto request, int userId)
    {
        // Validation: groupName is required
        if (string.IsNullOrEmpty(request.GroupName))
        {
            throw new ArgumentException("Group Name is required");
        }
        
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Check for duplicate group name
        var existingGroups = await _unitOfWork.Repository<Group>()
            .FindAsync(g => g.GroupName == request.GroupName && 
                           g.NetworkId == user.NetworkId && 
                           g.UserId == userId);
        
        if (existingGroups.Any())
        {
            throw new InvalidOperationException("Group already exist");
        }
        
        var group = new Group
        {
            GroupName = request.GroupName,
            NetworkId = user.NetworkId,
            UserId = userId,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<Group>().AddAsync(group);
        await _unitOfWork.SaveChangesAsync();
        
        // If contact list provided, add contacts to group
        if (!string.IsNullOrEmpty(request.Contact))
        {
            var result = await CreateBulkContactGroupAsync(
                group.Id, 
                user.NetworkId ?? 0, 
                request.Contact, 
                1, 
                userId);
            
            if (result)
            {
                // Success with contacts
                return MapToDto(group);
            }
            else
            {
                // Group created but contacts failed - still return success
                return MapToDto(group);
            }
        }
        
        return MapToDto(group);
    }
    
    public async Task<GroupDto> UpdateGroupAsync(UpdateGroupDto request, int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Handle two scenarios:
        // 1. Update group contacts only (name is empty, contacts provided)
        // 2. Update group name
        
        if (string.IsNullOrEmpty(request.GroupName) && 
            !string.IsNullOrEmpty(request.Contact) && 
            request.GroupId.HasValue && 
            request.GroupId.Value != 0)
        {
            // Scenario 1: Update contacts only
            var result = await CreateBulkContactGroupAsync(
                request.GroupId.Value,
                user.NetworkId ?? 0,
                request.Contact,
                1,
                userId);
            
            if (result)
            {
                var group = await _unitOfWork.Repository<Group>().GetByIdAsync(request.GroupId.Value);
                return MapToDto(group!);
            }
            else
            {
                throw new InvalidOperationException("unable to update the group contact with the selected contacts");
            }
        }
        
        // Scenario 2: Update group name
        var groupToUpdate = await _unitOfWork.Repository<Group>().GetByIdAsync(request.Id);
        
        if (groupToUpdate == null)
        {
            throw new KeyNotFoundException("Group not found");
        }
        
        groupToUpdate.GroupName = request.GroupName ?? groupToUpdate.GroupName;
        
        await _unitOfWork.Repository<Group>().UpdateAsync(groupToUpdate);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(groupToUpdate);
    }
    
    public async Task<GroupDto?> GetGroupByIdAsync(int id)
    {
        var group = await _unitOfWork.Repository<Group>().GetByIdAsync(id);
        if (group == null)
        {
            return null;
        }
        
        // Get ContactGroups with Contact details
        var contactGroups = await _unitOfWork.Repository<ContactGroup>()
            .FindAsync(cg => cg.GroupId == id);
        
        var contactIds = contactGroups
            .Select(cg => cg.ContactId ?? 0)
            .Where(id => id != 0)
            .ToList();
        
        var contacts = await _unitOfWork.Repository<Contact>()
            .FindAsync(c => contactIds.Contains(c.Id));
        
        // Note: Currently GroupDto doesn't have a Contacts property
        // In production, you'd extend GroupDto or create a GroupDetailDto
        return MapToDto(group);
    }
    
    public async Task<PaginatedGroupResponseDto> GetAllGroupsAsync(GetAllGroupsDto request, int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        var groups = await _unitOfWork.Repository<Group>()
            .FindAsync(g => g.NetworkId == user.NetworkId && g.UserId == userId);
        
        var groupsList = groups.ToList();
        
        // Get total count
        var total = groupsList.Count;
        
        // Apply pagination
        var pagedGroups = groupsList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        // For each group, get associated contacts (matching Node.js include behavior)
        var groupDtos = new List<GroupDto>();
        foreach (var group in pagedGroups)
        {
            // Get contact groups for this group
            var contactGroups = await _unitOfWork.Repository<ContactGroup>()
                .FindAsync(cg => cg.GroupId == group.Id);
            
            var contactIds = contactGroups
                .Select(cg => cg.ContactId ?? 0)
                .Where(id => id != 0)
                .ToList();
            
            // Get contacts for this group
            List<ContactDto> contacts = new();
            if (contactIds.Any())
            {
                var groupContacts = await _unitOfWork.Repository<Contact>()
                    .FindAsync(c => contactIds.Contains(c.Id));
                
                contacts = groupContacts.Select(c => new ContactDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Company = c.Company,
                    Country = c.Country,
                    NetworkId = c.NetworkId,
                    UserId = c.UserId,
                    StatusId = c.StatusId
                }).ToList();
            }
            
            var dto = MapToDto(group);
            dto.Contacts = contacts;
            groupDtos.Add(dto);
        }
        
        return new PaginatedGroupResponseDto
        {
            AllGroup = groupDtos,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    // Helper method to bulk create ContactGroup records
    private async Task<bool> CreateBulkContactGroupAsync(int groupId, int networkId, string contactData, int statusId, int userId)
    {
        try
        {
            // Parse contact objects from JSON array - expecting array of objects with 'id' property
            // Format: [{id: 1, ...}, {id: 2, ...}]
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(contactData);
            var jsonArray = jsonDoc.RootElement;
            
            var contactIds = new List<int>();
            
            foreach (var item in jsonArray.EnumerateArray())
            {
                // Try to get 'id' property (lowercase or PascalCase)
                if (item.TryGetProperty("id", out var idProp) || 
                    item.TryGetProperty("Id", out idProp))
                {
                    if (idProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        contactIds.Add(idProp.GetInt32());
                    }
                    else if (idProp.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        if (int.TryParse(idProp.GetString(), out var id))
                        {
                            contactIds.Add(id);
                        }
                    }
                }
            }
            
            if (!contactIds.Any())
            {
                return false;
            }
            
            foreach (var contactId in contactIds)
            {
                // Check if contact exists
                var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(contactId);
                if (contact == null)
                {
                    continue;
                }
                
                // Check if ContactGroup already exists
                var existingContactGroups = await _unitOfWork.Repository<ContactGroup>()
                    .FindAsync(cg => cg.ContactId == contactId && cg.GroupId == groupId);
                
                if (!existingContactGroups.Any())
                {
                    var contactGroup = new ContactGroup
                    {
                        ContactId = contactId,
                        GroupId = groupId,
                        StatusId = statusId
                    };
                    
                    await _unitOfWork.Repository<ContactGroup>().AddAsync(contactGroup);
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    public async Task<bool> DeleteGroupAsync(int id, int userId)
    {
        var group = await _unitOfWork.Repository<Group>().GetByIdAsync(id);
        
        if (group == null)
        {
            return false;
        }
        
        // Verify user owns this group or has access
        if (group.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this group");
        }
        
        await _unitOfWork.Repository<Group>().DeleteAsync(group);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    private static GroupDto MapToDto(Group group)
    {
        return new GroupDto
        {
            Id = group.Id,
            GroupName = group.GroupName ?? "",
            Description = null,
            NetworkId = group.NetworkId,
            UserId = group.UserId,
            StatusId = group.StatusId
        };
    }
}

public class ContactGroupService : IContactGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ContactGroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ContactGroupDto> CreateContactGroupAsync(CreateContactGroupDto request, int userId)
    {
        var contactGroup = new ContactGroup
        {
            ContactId = request.ContactId,
            GroupId = request.GroupId,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<ContactGroup>().AddAsync(contactGroup);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(contactGroup);
    }
    
    public async Task<ContactGroupDto> UpdateContactGroupAsync(UpdateContactGroupDto request)
    {
        var contactGroup = await _unitOfWork.Repository<ContactGroup>().GetByIdAsync(request.Id);
        
        if (contactGroup == null)
        {
            throw new KeyNotFoundException("Contact group not found");
        }
        
        contactGroup.ContactId = request.ContactId;
        contactGroup.GroupId = request.GroupId;
        
        await _unitOfWork.Repository<ContactGroup>().UpdateAsync(contactGroup);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(contactGroup);
    }
    
    public async Task<ContactGroupDto?> GetContactGroupByIdAsync(int id)
    {
        var contactGroup = await _unitOfWork.Repository<ContactGroup>().GetByIdAsync(id);
        return contactGroup == null ? null : MapToDto(contactGroup);
    }
    
    public async Task<PaginatedContactGroupResponseDto> GetAllContactGroupsAsync(GetAllContactGroupsDto request, int userId)
    {
        var contactGroups = await _unitOfWork.Repository<ContactGroup>()
            .FindAsync(cg => (request.GroupId == null || cg.GroupId == request.GroupId) &&
                            (request.ContactId == null || cg.ContactId == request.ContactId));
        
        var contactGroupsList = contactGroups.ToList();
        
        // Get total count
        var total = contactGroupsList.Count;
        
        // Apply pagination
        var pagedContactGroups = contactGroupsList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();
        
        return new PaginatedContactGroupResponseDto
        {
            AllGroup = pagedContactGroups,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    public async Task<bool> DeleteContactGroupAsync(int id)
    {
        var contactGroup = await _unitOfWork.Repository<ContactGroup>().GetByIdAsync(id);
        
        if (contactGroup == null)
        {
            return false;
        }
        
        await _unitOfWork.Repository<ContactGroup>().DeleteAsync(contactGroup);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    private static ContactGroupDto MapToDto(ContactGroup contactGroup)
    {
        return new ContactGroupDto
        {
            Id = contactGroup.Id,
            ContactId = contactGroup.ContactId ?? 0,
            GroupId = contactGroup.GroupId ?? 0,
            StatusId = contactGroup.StatusId
        };
    }
}

public class UserProfileService : IUserProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public UserProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UserProfileDto2> CreateUserProfileAsync(CreateUserProfileDto request, int userId)
    {
        var profile = new UserProfile
        {
            FirstName = request.FirstName ?? "",
            LastName = request.LastName ?? "",
            MiddleName = request.MiddleName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Picture = request.Picture,
            Company = request.Company,
            Country = request.Country,
            UserId = userId,
            StatusId = 1
        };
        
        await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(profile);
    }
    
    public async Task<UserProfileDto2> UpdateUserProfileAsync(UpdateUserProfileDto request, int userId)
    {
        var profile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(request.Id);
        
        if (profile == null)
        {
            throw new KeyNotFoundException("User profile not found");
        }
        
        profile.FirstName = request.FirstName ?? profile.FirstName;
        profile.LastName = request.LastName ?? profile.LastName;
        profile.MiddleName = request.MiddleName ?? profile.MiddleName;
        profile.PhoneNumber = request.PhoneNumber ?? profile.PhoneNumber;
        profile.Address = request.Address ?? profile.Address;
        
        await _unitOfWork.Repository<UserProfile>().UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(profile);
    }
    
    public async Task<UserProfileDto2?> GetUserProfileByIdAsync(int id)
    {
        // FIXED: Query by userId instead of id to match Node.js
        var profiles = await _unitOfWork.Repository<UserProfile>()
            .FindAsync(p => p.UserId == id);
        
        var profile = profiles.FirstOrDefault();
        if (profile == null)
        {
            return null;
        }
        
        // Fetch related User entity
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(profile.UserId);
        
        // TODO: Fetch Guarantors and Leaves when those services are implemented
        // var guarantors = await _unitOfWork.Repository<Guarantor>().FindAsync(g => g.UserProfileId == profile.Id);
        // var leaves = await _unitOfWork.Repository<Leave>().FindAsync(l => l.UserProfileId == profile.Id);
        
        return MapToDtoWithRelations(profile, user);
    }
    
    public async Task<IEnumerable<UserProfileDto2>> GetAllUserProfilesAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // FIXED: Implement userType-based filtering to match Node.js
        // If userTypeId is 1 or 2 (admin/super admin), show all profiles
        // Otherwise, filter by networkId
        
        IEnumerable<UserProfile> profiles;
        
        if (user.UserTypeId == 1 || user.UserTypeId == 2)
        {
            // Admin users see all profiles
            var allProfiles = await _unitOfWork.Repository<UserProfile>().GetAllAsync();
            profiles = allProfiles;
        }
        else
        {
            // Non-admin users only see profiles from their network
            var allProfiles = await _unitOfWork.Repository<UserProfile>().GetAllAsync();
            var allUsers = await _unitOfWork.Repository<User>().GetAllAsync();
            
            var userIds = allUsers
                .Where(u => u.NetworkId == user.NetworkId)
                .Select(u => u.Id)
                .ToList();
            
            profiles = allProfiles.Where(p => userIds.Contains(p.UserId));
        }
        
        // TODO: In production, enhance Repository pattern to support .Include()
        // This would allow including User, Guarantor, and Leave relations like Node.js
        // Also pagination should be applied: Skip(pageIndex).Take(pageSize)
        
        return profiles.Select(MapToDto);
    }
    
    private static UserProfileDto2 MapToDto(UserProfile profile)
    {
        return new UserProfileDto2
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            MiddleName = profile.MiddleName,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            Picture = profile.Picture,
            Company = profile.Company,
            Country = profile.Country,
            UserId = profile.UserId,
            StatusId = profile.StatusId
        };
    }
    
    private static UserProfileDto2 MapToDtoWithRelations(UserProfile profile, User? user)
    {
        return new UserProfileDto2
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            MiddleName = profile.MiddleName,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            Picture = profile.Picture,
            Company = profile.Company,
            Country = profile.Country,
            UserId = profile.UserId,
            StatusId = profile.StatusId,
            User = user == null ? null : new UserInfoDto
            {
                Email = user.Email,
                UserTypeId = user.UserTypeId,
                ProviderTypeId = user.ProviderTypeId,
                StatusId = user.StatusId,
                NetworkId = user.NetworkId
            },
            Guarantors = new List<object>(),
            Leaves = new List<object>()
        };
    }
}

public class SmtpSetupService : ISmtpSetupService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public SmtpSetupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<SmtpSetupDto> CreateSmtpSetupAsync(CreateSmtpSetupDto request, int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        
        var setup = new SmtpSetup
        {
            Host = request.Host,
            Port = request.Port,
            Username = request.Username,
            Password = request.Password,
            IsSecure = request.IsSecure,
            FromEmail = request.FromEmail,
            FromName = request.FromName,
            Encryption = request.Encryption,
            BatchSize = request.BatchSize,
            BatchIntervalMinutes = request.BatchIntervalMinutes,
            MaxRetryAttempts = request.MaxRetryAttempts,
            IsSendingEnabled = request.IsSendingEnabled,
            NetworkId = user?.NetworkId
        };
        
        await _unitOfWork.Repository<SmtpSetup>().AddAsync(setup);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(setup);
    }
    
    public async Task<SmtpSetupDto> UpdateSmtpSetupAsync(UpdateSmtpSetupDto request)
    {
        var setup = await _unitOfWork.Repository<SmtpSetup>().GetByIdAsync(request.Id);
        
        if (setup == null)
        {
            throw new KeyNotFoundException("SMTP setup not found");
        }
        
        setup.Host = request.Host;
        setup.Port = request.Port;
        setup.Username = request.Username;
        setup.Password = request.Password;
        setup.IsSecure = request.IsSecure;
        setup.FromEmail = request.FromEmail;
        setup.FromName = request.FromName;
        setup.Encryption = request.Encryption;
        setup.BatchSize = request.BatchSize;
        setup.BatchIntervalMinutes = request.BatchIntervalMinutes;
        setup.MaxRetryAttempts = request.MaxRetryAttempts;
        setup.IsSendingEnabled = request.IsSendingEnabled;
        
        await _unitOfWork.Repository<SmtpSetup>().UpdateAsync(setup);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(setup);
    }
    
    public async Task<SmtpSetupDto?> GetSmtpSetupByIdAsync(int id)
    {
        var setup = await _unitOfWork.Repository<SmtpSetup>().GetByIdAsync(id);
        return setup == null ? null : MapToDto(setup);
    }
    
    public async Task<PaginatedSmtpSetupResponseDto> GetAllSmtpSetupsAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        
        var setups = await _unitOfWork.Repository<SmtpSetup>()
            .GetAllAsync();
        
        var smtpSetupList = setups.Select(MapToDto).ToList();
        
        return new PaginatedSmtpSetupResponseDto
        {
            AllSmtpSetup = smtpSetupList,
            Page = 1,
            Total = smtpSetupList.Count
        };
    }
    
    public async Task<bool> DeleteSmtpSetupAsync(int id)
    {
        var setup = await _unitOfWork.Repository<SmtpSetup>().GetByIdAsync(id);
        
        if (setup == null)
        {
            return false;
        }
        
        await _unitOfWork.Repository<SmtpSetup>().DeleteAsync(setup);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<SmtpSetupDto?> GetSmtpSetupByNetworkIdAsync(int networkId)
    {
        var setups = await _unitOfWork.Repository<SmtpSetup>()
            .FindAsync(s => s.NetworkId == networkId);
        
        var setup = setups.FirstOrDefault();
        return setup == null ? null : MapToDto(setup);
    }
    
    private static SmtpSetupDto MapToDto(SmtpSetup setup)
    {
        return new SmtpSetupDto
        {
            Id = setup.Id,
            Host = setup.Host,
            Port = setup.Port,
            Username = setup.Username,
            Password = setup.Password,
            IsSecure = setup.IsSecure,
            FromEmail = setup.FromEmail,
            FromName = setup.FromName,
            Encryption = setup.Encryption,
            BatchSize = setup.BatchSize,
            BatchIntervalMinutes = setup.BatchIntervalMinutes,
            MaxRetryAttempts = setup.MaxRetryAttempts,
            IsSendingEnabled = setup.IsSendingEnabled,
            NetworkId = setup.NetworkId
        };
    }
}

public class SecuritySetupService : ISecuritySetupService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public SecuritySetupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<SecuritySetupDto> CreateSecuritySetupAsync(CreateSecuritySetupDto request, int userId)
    {
       
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            
            var setup = new SecuritySetup
            {
                Parameters = request.Parameters,
                PlatformSetup = request.PlatformSetup,
                ApprovedPage = request.ApprovedPage,
                RejectedPage = request.RejectedPage,
                IsMonitor = request.IsMonitor,
                NetworkId = request.NetworkId,
                StatusId = 1
            };
            
            await _unitOfWork.Repository<SecuritySetup>().AddAsync(setup);
            await _unitOfWork.SaveChangesAsync();
            
            return MapToDto(setup);
       
    }
    
    public async Task<SecuritySetupDto> UpdateSecuritySetupAsync(UpdateSecuritySetupDto request)
    {
        var setup = await _unitOfWork.Repository<SecuritySetup>().GetByIdAsync(request.Id);
        
        if (setup == null)
        {
            throw new KeyNotFoundException("Security setup not found");
        }
        
        setup.Parameters = request.Parameters;
        setup.PlatformSetup = request.PlatformSetup;
        setup.ApprovedPage = request.ApprovedPage;
        setup.RejectedPage = request.RejectedPage;
        setup.IsMonitor = request.IsMonitor;
        
        await _unitOfWork.Repository<SecuritySetup>().UpdateAsync(setup);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(setup);
    }
    
    public async Task<SecuritySetupDto?> GetSecuritySetupByIdAsync(int id)
    {
        var setup = await _unitOfWork.Repository<SecuritySetup>().GetByIdAsync(id);
        return setup == null ? null : MapToDto(setup);
    }
    
    public async Task<PaginatedSecuritySetupResponseDto> GetAllSecuritySetupsAsync(GetAllSecuritySetupsDto request)
    {
        // Query by networkId matching Node.js implementation
        var query = await _unitOfWork.Repository<SecuritySetup>()
            .FindAsync(s => s.Id == request.NetworkId);
        
        var total = query.Count();
        
        // Apply pagination
        var setups = query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();
        
        return new PaginatedSecuritySetupResponseDto
        {
            AllSetup = setups,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    public async Task<SecuritySetupDto> UpdateIsMonitorAsync(UpdateIsMonitorDto request)
    {
        var setup = await _unitOfWork.Repository<SecuritySetup>().GetByIdAsync(request.SetupId);
        
        if (setup == null)
        {
            throw new KeyNotFoundException("Security setup not found");
        }
        
        setup.IsMonitor = request.IsMonitor;
        
        await _unitOfWork.Repository<SecuritySetup>().UpdateAsync(setup);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(setup);
    }
    
    public async Task<SecuritySetupDto?> GetSecuritySetupByNetworkIdAsync(int networkId)
    {
        var setups = await _unitOfWork.Repository<SecuritySetup>()
            .FindAsync(s => s.NetworkId == networkId);
        
        var setup = setups.FirstOrDefault();
        return setup == null ? null : MapToDto(setup);
    }
    
    private static SecuritySetupDto MapToDto(SecuritySetup setup)
    {
        return new SecuritySetupDto
        {
            Id = setup.Id,
            Parameters = setup.Parameters,
            PlatformSetup = setup.PlatformSetup,
            ApprovedPage = setup.ApprovedPage,
            RejectedPage = setup.RejectedPage,
            IsMonitor = setup.IsMonitor,
            NetworkId = setup.NetworkId,
            StatusId = setup.StatusId
        };
    }
}
