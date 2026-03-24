namespace TechSolutions_IPS_HW.Models.Audit;

public class AuditEntry
{
    public int Id { get; set; }

    public string Action { get; set; } = null!;

    /// <summary>
    /// FK to the <see cref="AuditActor"/> of the user this action relates to (subject)
    /// </summary>
    public int? SubjectActorId { get; set; }

    public AuditActor? SubjectActor { get; set; }

    /// <summary>
    /// FK to the <see cref="AuditActor"/> of the person who performed the action
    /// </summary>
    public int? PerformerActorId { get; set; }

    public AuditActor? PerformerActor { get; set; }

    /// <summary>
    /// Optional FK to the customer this action relates to
    /// </summary>
    public Guid? CustomerId { get; set; }

    public Customer.Customer? Customer { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string? Details { get; set; }
}