namespace MyPortal.Application.DTOs;

public class ContactDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Skype { get; set; }
    public string? Country { get; set; }
    public string? Manager { get; set; }
    public int? NetworkId { get; set; }
    public int? UserId { get; set; }
    public int StatusId { get; set; }
}

public class CreateContactDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Skype { get; set; }
    public string? Country { get; set; }
    public string? Manager { get; set; }
}

public class UpdateContactDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class GetAllContactsDto
{
    public int StatusId { get; set; }
    public int? NetworkId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UploadContactDto
{
    public string Data { get; set; } = string.Empty; // JSON array of contacts
    public int? GroupId { get; set; } // Optional group to assign contacts to
    public int NetworkId { get; set; }
}

public class UploadResultDto
{
    public List<object> Duplicate { get; set; } = new();
    public List<object> Invalid { get; set; } = new();
}

public class PaginatedContactResponseDto
{
    public List<ContactDto> AllContact { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}

public class EmailCampaignDto
{
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TemplateName { get; set; }
    public Dictionary<string, string>? TemplateData { get; set; }
}
