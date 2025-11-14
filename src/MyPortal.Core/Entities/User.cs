namespace MyPortal.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int UserTypeId { get; set; }
    public string? ProviderTypeId { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public UserType UserType { get; set; } = null!;
    public Network? Network { get; set; }
    public Status Status { get; set; } = null!;
    public UserProfile? UserProfile { get; set; }
    public ICollection<Token> Tokens { get; set; } = new List<Token>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
