using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Audit;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Repositories;

/// <summary>
///Implementation of <see cref="IAuditRepository"/>.
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _db;

    public AuditRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(AuditEntry entry)
    {
        _db.AuditEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetCreatedCustomerIdsByPerformerAsync(string performerUserId)
    {
        return await _db.AuditEntries
            .AsNoTracking()
            .Where(a => a.Action == "CustomerCreated"
                        && a.CustomerId.HasValue
                        && a.PerformerActor != null
                        && a.PerformerActor.UserId == performerUserId)
            .Select(a => a.CustomerId!.Value)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Guid?> GetLastCreatedCustomerIdByPerformerAsync(string performerUserId)
    {
        return await _db.AuditEntries
            .AsNoTracking()
            .Where(a => a.Action == "CustomerCreated"
                        && a.CustomerId.HasValue
                        && a.PerformerActor != null
                        && a.PerformerActor.UserId == performerUserId)
            .OrderByDescending(a => a.TimestampUtc)
            .Select(a => a.CustomerId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasCreatedCustomerAsync(Guid customerId, string performerUserId)
    {
        return await _db.AuditEntries
            .AsNoTracking()
            .AnyAsync(a => a.Action == "CustomerCreated"
                           && a.CustomerId == customerId
                           && a.PerformerActor != null
                           && a.PerformerActor.UserId == performerUserId);
    }

    public async Task<Dictionary<Guid, string>> GetCustomerCreatorDisplayNamesAsync(
        IEnumerable<Guid> customerIds)
    {
        var ids = customerIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();

        var entries = await _db.AuditEntries
            .AsNoTracking()
            .Where(a => a.Action == "CustomerCreated"
                        && a.CustomerId.HasValue
                        && ids.Contains(a.CustomerId.Value)
                        && a.PerformerActor != null)
            .OrderBy(a => a.TimestampUtc)
            .Select(a => new
            {
                CustomerId = a.CustomerId!.Value,
                a.PerformerActor!.DisplayName
            })
            .ToListAsync();

        return entries
            .GroupBy(x => x.CustomerId)
            .ToDictionary(g => g.Key, g => g.First().DisplayName);
    }
}
