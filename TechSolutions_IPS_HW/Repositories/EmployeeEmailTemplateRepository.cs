using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Repositories;

public class EmployeeEmailTemplateRepository : IEmployeeEmailTemplateRepository
{
    private readonly ApplicationDbContext _db;

    public EmployeeEmailTemplateRepository(ApplicationDbContext db)
    {
        _db = db;
    }


    //Load email templates associated with a specific user
    public Task<List<EmployeeEmailTemplate>> GetByOwnerAsync(string ownerUserId)
        => _db.EmployeeEmailTemplates
            .AsNoTracking()
            .Where(t => t.OwnerUserId == ownerUserId)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public Task<EmployeeEmailTemplate?> GetByIdForOwnerAsync(Guid id, string ownerUserId)
        => _db.EmployeeEmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerUserId == ownerUserId);

    public Task<bool> ExistsNameAsync(string ownerUserId, string name, Guid? excludeId = null)
        => _db.EmployeeEmailTemplates.AnyAsync(t =>
            t.OwnerUserId == ownerUserId
            && t.Name == name
            && (!excludeId.HasValue || t.Id != excludeId.Value));

    public async Task AddAsync(EmployeeEmailTemplate template)
    {
        _db.EmployeeEmailTemplates.Add(template);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmployeeEmailTemplate template)
    {
        _db.EmployeeEmailTemplates.Update(template);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(EmployeeEmailTemplate template)
    {
        _db.EmployeeEmailTemplates.Remove(template);
        await _db.SaveChangesAsync();
    }
}
