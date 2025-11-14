namespace MyPortal.Application.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? NetworkId { get; set; }
    public int? UserId { get; set; }
    public int StatusId { get; set; }
    public List<ContactDto>? Contacts { get; set; }
}

public class PaginatedGroupResponseDto
{
    public List<GroupDto> AllGroup { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}

public class CreateGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Contact { get; set; } // JSON array of contact IDs to add to group
}

public class UpdateGroupDto
{
    public int Id { get; set; }
    public int? GroupId { get; set; } // For updating group contacts
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public string? Contact { get; set; } // JSON array of contact IDs to add to group
}

public class GetAllGroupsDto
{
    public int? NetworkId { get; set; }
    public int? StatusId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
