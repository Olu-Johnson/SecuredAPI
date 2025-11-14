using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

public class SmtpSetup : BaseEntity
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    
    [Column("isSecure")]
    public bool IsSecure { get; set; }
    
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? Encryption { get; set; }
    
    // Batch sending configuration
    public int BatchSize { get; set; } = 50;
    public int BatchIntervalMinutes { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 5;
    public bool IsSendingEnabled { get; set; } = true;
    
    public int? NetworkId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
}
