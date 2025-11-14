namespace MyPortal.Core.Entities;

public class Group : BaseEntity
{
    public string GroupName { get; set; } = string.Empty;
    public int? NetworkId { get; set; }
    public int? UserId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
    public User? User { get; set; }
    public Status Status { get; set; } = null!;
    public ICollection<ContactGroup> ContactGroups { get; set; } = new List<ContactGroup>();
}
