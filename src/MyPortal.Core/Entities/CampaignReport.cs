using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

public class CampaignReport : BaseEntity
{
    public string? ClickTime { get; set; }
    public string? Email { get; set; }
    
    [Column("firstname")]
    public string? FirstName { get; set; }
    
    [Column("lastname")]
    public string? LastName { get; set; }
    
    public string? Campaign { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? ClickIP { get; set; }
    public string? Country { get; set; }
    public string? OsName { get; set; }
    public string? DeviceType { get; set; }
    
    [Column("broswerName")]
    public string? BrowserName { get; set; }
    
    public int? NetworkId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
}
