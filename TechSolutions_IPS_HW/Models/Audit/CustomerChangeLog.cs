namespace TechSolutions_IPS_HW.Models.Audit;

/// <summary>
/// Records old and new field values whenever a cstomer is created, updated or deleted.
/// Values are stored as JSON objects for tracking.
/// </summary>
public class CustomerChangeLog
{
    public long Id { get; set; }

    /// <summary>
    /// FK to the customer this change relates to
    /// </summary>
    public Guid CustomerId { get; set; }

    public Customer.Customer? Customer { get; set; }

    /// <summary>
    /// The type of change: Created, Updated, Deleted
    /// </summary>
    public string ChangeType { get; set; } = null!;

    /// <summary>
    /// JSON object of the field values before the change.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON object of the field values after the change.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// FK to the <see cref="AuditActor"/> who made the change
    /// </summary>
    public int? PerformerActorId { get; set; }

    public AuditActor? PerformerActor { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
