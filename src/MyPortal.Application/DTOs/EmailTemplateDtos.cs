namespace MyPortal.Application.DTOs;

public class EmailTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Html"; // Html or Plain
    public string Module { get; set; } = "Default"; // Default, Offer, Campaign
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEmailTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Html"; // Html or Plain
    public string Module { get; set; } = "Default"; // Default, Offer, Campaign
    public int? NetworkId { get; set; }
}

public class UpdateEmailTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Html"; // Html or Plain
    public string Module { get; set; } = "Default"; // Default, Offer, Campaign
}

public class GetAllEmailTemplatesDto
{
    public int? NetworkId { get; set; }
    public int? StatusId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedEmailTemplateResponseDto
{
    public List<EmailTemplateDto> AllTemplate { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}
