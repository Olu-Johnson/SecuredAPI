namespace MyPortal.Core.Entities;

public class ContactGroup : BaseEntity
{
    public int? ContactId { get; set; }
    public int? GroupId { get; set; }
    public int? NetworkId { get; set; }
    public int? UserId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Contact? Contact { get; set; }
    public Group? Group { get; set; }
    public Network? Network { get; set; }
    public User? User { get; set; }
    public Status Status { get; set; } = null!;
}
