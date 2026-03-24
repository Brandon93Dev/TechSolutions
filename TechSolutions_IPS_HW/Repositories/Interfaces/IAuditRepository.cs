using TechSolutions_IPS_HW.Models.Audit;

namespace TechSolutions_IPS_HW.Repositories.Interfaces;

/// <summary>
/// Provides data access operations for <see cref="AuditEntry"/> entities
/// </summary>
public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry);

    Task<List<Guid>> GetCreatedCustomerIdsByPerformerAsync(string performerUserId);

    Task<Guid?> GetLastCreatedCustomerIdByPerformerAsync(string performerUserId);

    Task<bool> HasCreatedCustomerAsync(Guid customerId, string performerUserId);

    Task<Dictionary<Guid, string>> GetCustomerCreatorDisplayNamesAsync(
        IEnumerable<Guid> customerIds);
}
