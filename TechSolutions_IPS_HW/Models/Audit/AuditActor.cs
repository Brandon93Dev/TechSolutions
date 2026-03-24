using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Models.Audit;

/// <summary>
/// Intersection entity that captures a snapshot of a user at the time of an audit event.
/// Even if the <see cref="ApplicationUser"/> is later soft-deleted or hard-deleted,
/// this record preserves who performed (or was the subject of) the action.
/// </summary>
public class AuditActor
{
    public int Id { get; set; }

    /// <summary>
    /// The Identity user ID at the time of capture.
    /// </summary>
    public string? UserId { get; set; }
    
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Snapshot of the user's display name at capture time.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot of the user's email at capture time.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this snapshot was taken.
    /// </summary>
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<AuditEntry> SubjectEntries { get; set; } = new List<AuditEntry>();
    
    public ICollection<AuditEntry> PerformerEntries { get; set; } = new List<AuditEntry>();
}
