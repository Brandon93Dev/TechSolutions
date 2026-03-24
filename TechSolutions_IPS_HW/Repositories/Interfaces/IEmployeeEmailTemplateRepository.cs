using TechSolutions_IPS_HW.Models.Email;

namespace TechSolutions_IPS_HW.Repositories.Interfaces;

public interface IEmployeeEmailTemplateRepository
{
    Task<List<EmployeeEmailTemplate>> GetByOwnerAsync(string ownerUserId);
    
    Task<EmployeeEmailTemplate?> GetByIdForOwnerAsync(Guid id, string ownerUserId);
    
    Task<bool> ExistsNameAsync(string ownerUserId, string name, Guid? excludeId = null);
    
    Task AddAsync(EmployeeEmailTemplate template);
    
    Task UpdateAsync(EmployeeEmailTemplate template);
    
    Task DeleteAsync(EmployeeEmailTemplate template);
}
