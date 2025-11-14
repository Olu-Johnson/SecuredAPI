namespace MyPortal.Core.Entities;

public class Token : BaseEntity
{
    public int Value { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime ExpiredAt { get; set; }
    public int UserId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Status Status { get; set; } = null!;
}
