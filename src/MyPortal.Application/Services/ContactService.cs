using MyPortal.Application.DTOs;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace MyPortal.Application.Services;

public class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ContactService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ContactDto> CreateContactAsync(CreateContactDto request, int userId)
    {
        // Validation: firstname is required
        if (string.IsNullOrEmpty(request.FirstName))
        {
            throw new ArgumentException("firstname is required");
        }
        
        // Validation: lastname is required
        if (string.IsNullOrEmpty(request.LastName))
        {
            throw new ArgumentException("lastname is required");
        }
        
        // Validation: email is required
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("email is required");
        }
        
        // Validation: validate email format
        if (!IsValidEmail(request.Email))
        {
            throw new ArgumentException("Invalid email address");
        }
        
        // Get user to find their network
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Check if contact with same email already exists for this user/network
        var existingContacts = await _unitOfWork.Repository<Contact>()
            .FindAsync(c => c.Email == request.Email && 
                           c.NetworkId == user.NetworkId && 
                           c.UserId == userId);
        
        if (existingContacts.Any())
        {
            throw new InvalidOperationException("email already exist");
        }
        
        var contact = new Contact
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Skype = request.Skype,
            Country = request.Country,
            Manager = request.Manager,
            NetworkId = user?.NetworkId,
            UserId = userId,
            StatusId = 1 // Active
        };
        
        await _unitOfWork.Repository<Contact>().AddAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(contact);
    }
    
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Simple email validation regex
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<ContactDto> UpdateContactAsync(UpdateContactDto request, int userId)
    {
        var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(request.Id);
        
        if (contact == null)
        {
            throw new KeyNotFoundException("Contact not found");
        }
        
        // Verify user owns this contact or has access
        if (contact.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this contact");
        }
        
        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        
        await _unitOfWork.Repository<Contact>().UpdateAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(contact);
    }
    
    public async Task<ContactDto?> GetContactByIdAsync(int id)
    {
        var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(id);
        return contact == null ? null : MapToDto(contact);
    }
    
    public async Task<PaginatedContactResponseDto> GetAllContactsAsync(GetAllContactsDto request, int userId)
    {
        // Get user to find their network
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Get contacts filtered by networkId and userId
        var contacts = await _unitOfWork.Repository<Contact>()
            .FindAsync(c => c.NetworkId == user.NetworkId && c.UserId == userId);
        
        var contactsList = contacts.ToList();
        
        // Get total count
        var total = contactsList.Count;
        
        // Apply pagination
        var pagedContacts = contactsList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();
        
        return new PaginatedContactResponseDto
        {
            AllContact = pagedContacts,
            Page = request.PageIndex,
            Total = total
        };
    }
    
    public async Task<UploadResultDto> UploadContactsAsync(UploadContactDto request, int userId)
    {
        var invalidDetails = new List<object>();
        var duplicates = new List<object>();
        var validList = new List<Contact>();
        var validGroup = new List<ContactGroup>();
        
        // Get user to find their network
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Check if Data is empty or null
        if (string.IsNullOrWhiteSpace(request.Data))
        {
            return new UploadResultDto
            {
                Duplicate = duplicates,
                Invalid = invalidDetails
            };
        }
        
        // Parse the data (expecting JSON array of contacts)
        List<CreateContactDto>? dataList;
        try
        {
            // Use dynamic parsing to handle numbers as strings
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(request.Data);
            var jsonArray = jsonDoc.RootElement;
            
            dataList = new List<CreateContactDto>();
            
            foreach (var item in jsonArray.EnumerateArray())
            {
                var contact = new CreateContactDto
                {
                    FirstName = item.TryGetProperty("firstname", out var fname) ? fname.GetString() ?? "" :
                               item.TryGetProperty("FirstName", out var fname2) ? fname2.GetString() ?? "" : "",
                    LastName = item.TryGetProperty("lastname", out var lname) ? lname.GetString() ?? "" :
                              item.TryGetProperty("LastName", out var lname2) ? lname2.GetString() ?? "" : "",
                    Email = item.TryGetProperty("email", out var email) ? email.GetString() ?? "" :
                           item.TryGetProperty("Email", out var email2) ? email2.GetString() ?? "" : "",
                    Phone = item.TryGetProperty("phone", out var phone) ? 
                           (phone.ValueKind == System.Text.Json.JsonValueKind.Number ? phone.GetInt64().ToString() : phone.GetString()) :
                           item.TryGetProperty("Phone", out var phone2) ? 
                           (phone2.ValueKind == System.Text.Json.JsonValueKind.Number ? phone2.GetInt64().ToString() : phone2.GetString()) : null,
                    Company = item.TryGetProperty("company", out var comp) ? comp.GetString() :
                             item.TryGetProperty("Company", out var comp2) ? comp2.GetString() : null,
                    Country = item.TryGetProperty("country", out var country) ? country.GetString() :
                             item.TryGetProperty("Country", out var country2) ? country2.GetString() : null
                };
                
                dataList.Add(contact);
            }
        }
        catch (Exception e)
        {
            return new UploadResultDto
            {
                Duplicate = duplicates,
                Invalid = new List<object> { new { reason = $"Invalid JSON format: {e.Message}" } }
            };
        }
        
        if (dataList == null || !dataList.Any())
        {
            return new UploadResultDto
            {
                Duplicate = duplicates,
                Invalid = invalidDetails
            };
        }
        
        // Validate each contact
        foreach (var element in dataList)
        {
            // Check for required fields
            if (string.IsNullOrEmpty(element.Email) || 
                string.IsNullOrEmpty(element.FirstName) || 
                string.IsNullOrEmpty(element.LastName))
            {
                invalidDetails.Add(new 
                { 
                    email = element.Email ?? "",
                    firstname = element.FirstName ?? "",
                    lastname = element.LastName ?? "",
                    reason = "either email, firstname or lastname can not be empty" 
                });
                continue;
            }
            
            // Validate email format
            if (!IsValidEmail(element.Email))
            {
                invalidDetails.Add(new 
                { 
                    email = element.Email,
                    firstname = element.FirstName,
                    lastname = element.LastName,
                    reason = "email is not valid" 
                });
                continue;
            }
            
            // Add to valid list
            validList.Add(new Contact
            {
                Email = element.Email,
                FirstName = element.FirstName,
                LastName = element.LastName,
                Phone = element.Phone,
                Company = element.Company,
                Country = element.Country,
                StatusId = 1,
                NetworkId = request.NetworkId,
                UserId = userId
            });
        }
        
        // Check for existing contacts (duplicates)
        var validEmails = validList.Select(c => c.Email).ToList();
        var existingContacts = await _unitOfWork.Repository<Contact>()
            .FindAsync(c => validEmails.Contains(c.Email) && c.NetworkId == request.NetworkId);
        
        var existingContactsList = existingContacts.ToList();
        
        // Add duplicates to list
        foreach (var existing in existingContactsList)
        {
            duplicates.Add(new 
            { 
                email = existing.Email,
                firstname = existing.FirstName,
                lastname = existing.LastName,
                reason = "email already exist" 
            });
        }
        
        // Filter out duplicates - only insert contacts that don't already exist
        var uniqueContacts = validList
            .Where(v => !existingContactsList.Any(e => e.Email == v.Email))
            .ToList();
        
        // Bulk insert unique contacts
        if (uniqueContacts.Any())
        {
            foreach (var contact in uniqueContacts)
            {
                await _unitOfWork.Repository<Contact>().AddAsync(contact);
            }
            await _unitOfWork.SaveChangesAsync();
        }
        
        // If groupId is provided, add contacts to the group
        if (request.GroupId.HasValue && request.GroupId.Value > 0)
        {
            // Get all contacts from the dataList (both newly added and existing)
            foreach (var element in dataList)
            {
                // Find the contact by email
                var contactResult = await _unitOfWork.Repository<Contact>()
                    .FindAsync(c => c.Email == element.Email && c.NetworkId == request.NetworkId);
                
                var contact = contactResult.FirstOrDefault();
                if (contact != null)
                {
                    // Check if contact is already in the group
                    var existingInGroup = await _unitOfWork.Repository<ContactGroup>()
                        .FindAsync(cg => cg.ContactId == contact.Id && 
                                        cg.GroupId == request.GroupId.Value);
                    
                    if (!existingInGroup.Any())
                    {
                        var contactGroup = new ContactGroup
                        {
                            ContactId = contact.Id,
                            GroupId = request.GroupId.Value,
                            StatusId = 1
                        };
                        await _unitOfWork.Repository<ContactGroup>().AddAsync(contactGroup);
                    }
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
        }
        
        return new UploadResultDto
        {
            Duplicate = duplicates,
            Invalid = invalidDetails
        };
    }
    
    public async Task<bool> DeleteContactAsync(int id, int userId)
    {
        var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(id);
        
        if (contact == null)
        {
            return false;
        }
        
        // Verify user owns this contact or has access
        if (contact.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this contact");
        }
        
        await _unitOfWork.Repository<Contact>().DeleteAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<ContactDto> BlockContactAsync(int id, int userId)
    {
        var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(id);
        
        if (contact == null)
        {
            throw new KeyNotFoundException("Contact not found");
        }
        
        // Verify user owns this contact or has access
        if (contact.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to block this contact");
        }
        
        contact.StatusId = Constants.StatusConstants.Block;
        
        await _unitOfWork.Repository<Contact>().UpdateAsync(contact);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(contact);
    }
    
    private static ContactDto MapToDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Phone = contact.Phone,
            Company = contact.Company,
            Skype = contact.Skype,
            Country = contact.Country,
            Manager = contact.Manager,
            NetworkId = contact.NetworkId,
            UserId = contact.UserId,
            StatusId = contact.StatusId
        };
    }
}
