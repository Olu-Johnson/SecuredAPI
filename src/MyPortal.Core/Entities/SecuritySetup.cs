namespace MyPortal.Core.Entities;

public class SecuritySetup : BaseEntity
{
    public string? Parameters { get; set; }
    public string? PlatformSetup { get; set; }
    public string? ApprovedPage { get; set; }
    public string? RejectedPage { get; set; }
    public bool? IsMonitor { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
    public Status Status { get; set; } = null!;
}
