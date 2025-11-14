namespace MyPortal.Core.Entities;

public class Network : BaseEntity
{
    public string Company { get; set; } = string.Empty;
    public string? TaxDetails { get; set; }
    public string? AccountSecretKey { get; set; }
    public string? NetworkLogo { get; set; }
    public string? NetworkUrl { get; set; }
    public string NetworkName { get; set; } = string.Empty;
    public string? NetworkSignupUrl { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Status Status { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<CampaignDetails> Campaigns { get; set; } = new List<CampaignDetails>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
