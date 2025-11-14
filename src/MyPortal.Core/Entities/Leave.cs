namespace MyPortal.Core.Entities;

public class Leave : BaseEntity
{
    public int UserId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "pending";
    public int LeaveDays { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
