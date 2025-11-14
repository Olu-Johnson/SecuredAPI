namespace MyPortal.Core.Entities;

public class UserProfile : BaseEntity
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? SocialContact { get; set; }
    public string? CustomProfileDetails { get; set; }
    public string? TaxDetails { get; set; }
    public string? AccountSecretKey { get; set; }
    public string? Picture { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Status Status { get; set; } = null!;
}
