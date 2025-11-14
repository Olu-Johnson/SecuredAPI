namespace MyPortal.Core.Entities;

public class Email : BaseEntity
{
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public int? TypeId { get; set; }
    public int? RetryCount { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public DateTime? SentDate { get; set; }
    public string? Attachment { get; set; }
    public string? FileName { get; set; }
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
    public Status Status { get; set; } = null!;
}
