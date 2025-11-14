namespace MyPortal.Core.Entities;

public class EmailTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Html"; // Html or Plain
    public string Module { get; set; } = "Default"; // Default, Offer, Campaign
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
    public Status Status { get; set; } = null!;
}
