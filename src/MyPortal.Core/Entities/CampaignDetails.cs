namespace MyPortal.Core.Entities;

public class CampaignDetails : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string? PromotionLink { get; set; }
    public string? RejectedLink { get; set; }
    public string? Type { get; set; }
    public string? Content { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
    public Status Status { get; set; } = null!;
    public ICollection<CampaignReport> Reports { get; set; } = new List<CampaignReport>();
}
