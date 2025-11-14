namespace MyPortal.Core.Entities;

public class UserType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
