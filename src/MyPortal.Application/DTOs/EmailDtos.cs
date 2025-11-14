namespace MyPortal.Application.DTOs;

public class EmailDto
{
    public int Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int StatusId { get; set; }
    public int? NetworkId { get; set; }
}

public class CreateEmailDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? NetworkId { get; set; }
}

public class UpdateEmailDto
{
    public int Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SaveOfferEmailDto
{
    public int GroupId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Offers { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
}

public class SaveCampaignEmailDto
{
    public int GroupId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int CampaignId { get; set; }
    public string? Protocol { get; set; } // e.g., "https"
    public string? Host { get; set; } // e.g., "localhost:5108"
}

public class GetAllEmailsDto
{
    public int? NetworkId { get; set; }
    public int? StatusId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginatedEmailResponseDto
{
    public List<EmailDto> AllEmail { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}
