namespace MyPortal.Core.Entities;

public class Contact : BaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Skype { get; set; }
    public string? Country { get; set; }
    public string? Manager { get; set; }
    public int? NetworkId { get; set; }
    public int? UserId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
    public User? User { get; set; }
    public Status Status { get; set; } = null!;
    public ICollection<ContactGroup> ContactGroups { get; set; } = new List<ContactGroup>();
}
