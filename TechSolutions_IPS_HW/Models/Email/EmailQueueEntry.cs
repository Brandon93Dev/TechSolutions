using TechSolutions_IPS_HW.Models.Enums;

namespace TechSolutions_IPS_HW.Models.Email;

/// <summary>
/// Represents a queued email persisted in the database.
/// Emials remain in the table until successfully sent or max retries are exceeded.
/// </summary>
public class EmailQueueEntry
{
    public int Id { get; set; }

    public string ToAddress { get; set; } = string.Empty;
    
    public string Subject { get; set; } = string.Empty;
    
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// ENUM : Pending, Sent, or Failed.
    /// </summary>
    public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;

    public int RetryCount { get; set; }

    /// <summary>
    /// Last error message if sending failed.
    /// </summary>
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? SentAt { get; set; }
    
    public DateTime? NextRetryAt { get; set; }
}
