using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Repositories;

/// <summary>
/// Implementation of <see cref="IEmailQueueRepository"/>.
/// </summary>
public class EmailQueueRepository : IEmailQueueRepository
{
    private readonly ApplicationDbContext _db;

    public EmailQueueRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task EnqueueAsync(EmailQueueEntry entry)
    {
        _db.EmailQueueEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<EmailQueueEntry>>
        GetPendingBatchAsync(
        int batchSize, 
        DateTime utcNow, 
        CancellationToken ct = default)
    {
        return await _db.EmailQueueEntries
            .Where(e => 
                e.Status == EmailQueueStatus.Pending && 
                (e.NextRetryAt == null || e.NextRetryAt <= utcNow))
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(EmailQueueEntry entry, CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }


    //Had some issues using LINQ here for some reason so used raw sql call
    // TODO: Investigate and correct
    public async Task<int> RemoveDuplicatesAsync(CancellationToken ct = default)
    {
        return await _db.Database.ExecuteSqlRawAsync(
            """
            WITH Ranked AS (
                SELECT Id,
                       ROW_NUMBER() OVER (
                           PARTITION BY ToAddress, Subject, Body
                           ORDER BY CreatedAt DESC
                       ) AS RowNum
                FROM EmailQueueEntries
                WHERE Status = 0
            )
            DELETE FROM Ranked WHERE RowNum > 1
            """, ct);
    }
}
