using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Models.ViewModels;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface IEmployeeEmailTemplateService
{
    Task<List<EmployeeEmailTemplate>> GetMyTemplatesAsync(string ownerUserId);
    
    Task<EmployeeEmailTemplate?> GetMyTemplateAsync(Guid id, string ownerUserId);
    
    Task<(bool Success, string Error)> CreateAsync(string ownerUserId, EmployeeEmailTemplateFormModel model);
    
    Task<(bool Success, string Error)> UpdateAsync(string ownerUserId, EmployeeEmailTemplateFormModel model);
    
    Task<(bool Success, string Error)> DeleteAsync(string ownerUserId, Guid id);
}
