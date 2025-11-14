namespace MyPortal.Core.Entities;

public class Guarantor : BaseEntity
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Relationship { get; set; }
    public string? DocumentPath { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
