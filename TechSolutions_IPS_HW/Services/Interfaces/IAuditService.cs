namespace TechSolutions_IPS_HW.Services.Interfaces;


/// <summary>
/// Interface to describe audit logging service that records user actions for security and AUDIT reasons.
/// Uses <see cref="Models.Audit.AuditActor"/> snapshots so audit history
/// survives user deletion.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit event, automatically capturing actor snapshots.
    /// </summary>
    /// <param name="subjectUserId">Identity ID of the user the action relates to (User being approved's ID).</param>
    /// <param name="action">Short action name (e.g. "CustomerCreated", "UserApproved").</param>
    Task LogAsync(string subjectUserId, string action, string? details = null, Guid? customerId = null);

    /// <summary>
    /// Logs a customer change with old/new value snapshots stored as JSON.
    /// </summary>
    Task LogCustomerChangeAsync(Guid customerId, string changeType, string? oldValuesJson, string? newValuesJson);
}
