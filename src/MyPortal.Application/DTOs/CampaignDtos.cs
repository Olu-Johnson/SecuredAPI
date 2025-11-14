namespace MyPortal.Application.DTOs;

public class CampaignDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string? PromotionLink { get; set; }
    public string? RejectedLink { get; set; }
    public string? Type { get; set; }
    public string? Content { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
}

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? RejectedLink { get; set; }
    public string? Type { get; set; }
    public string? Content { get; set; }
    public int? NetworkId { get; set; }
}

public class CampaignReportDto
{
    public int Id { get; set; }
    public string? ClickTime { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Campaign { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? ClickIP { get; set; }
    public string? Country { get; set; }
    public string? OsName { get; set; }
    public string? DeviceType { get; set; }
    public string? BrowserName { get; set; }
    public int? NetworkId { get; set; }
}

public class UpdateCampaignDto : CreateCampaignDto
{
    public int Id { get; set; }
}

public class GetByIdDto
{
    public int Id { get; set; }
}

public class GetByNetworkIdDto
{
    public int NetworkId { get; set; }
}

public class GetAllCampaignsDto
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? StatusId { get; set; }
}

public class IPValidationResult
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // "Approved" or "Rejected"
    public string? Reason { get; set; }
    public string? Country { get; set; }
    public int ThreatScore { get; set; }
    public int TrustScore { get; set; }
    public bool IsVpn { get; set; }
    public bool IsProxy { get; set; }
    public bool IsTor { get; set; }
    public bool IsDatacenter { get; set; }
}

public class GetAllClicksReportDto
{
    public int NetworkId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ClicksReportDto
{
    public int Id { get; set; }
    public string? ClickTime { get; set; }
    public string Publisher { get; set; } = string.Empty; // Maps from Email
    public string Offer { get; set; } = string.Empty; // Maps from Campaign
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? ClickIP { get; set; }
    public string? Country { get; set; }
    public string OsName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string BroswerName { get; set; } = string.Empty; // Note: typo preserved from Node.js
    public int? NetworkId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PaginatedClicksReportResponseDto
{
    public List<ClicksReportDto> AllReport { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}

public class PaginatedCampaignResponseDto
{
    public List<CampaignDetailsDto> AllCampaign { get; set; } = new();
    public int Page { get; set; }
    public int Total { get; set; }
}
