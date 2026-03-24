using TechSolutions_IPS_HW.Models.Email;

namespace TechSolutions_IPS_HW.Repositories.Interfaces;

/// <summary>
/// Provides data access for <see cref="EmailQueueEntry"/>
/// </summary>
public interface IEmailQueueRepository
{
    Task EnqueueAsync(EmailQueueEntry entry);
    
    Task<IReadOnlyList<EmailQueueEntry>> GetPendingBatchAsync(
        int batchSize, 
        DateTime utcNow, 
        CancellationToken ct = default);
    
    Task UpdateAsync(EmailQueueEntry entry, CancellationToken ct = default);
    
    Task<int> RemoveDuplicatesAsync(CancellationToken ct = default);
}
