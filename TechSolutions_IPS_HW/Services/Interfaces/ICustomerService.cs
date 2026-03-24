using TechSolutions_IPS_HW.Models.Customer;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface ICustomerService
{
    Task<List<Customer>> GetCustomersAsync(string? status);
    
    Task<Customer?> GetByIdAsync(Guid id);

    Task<Dictionary<Guid, string>> GetCustomerCreatorDisplayNamesAsync(
        IEnumerable<Guid> customerIds);
    
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeCustomerId = null);
    
    Task<bool> ExistsByIdNumberAsync(string idNumber);
    
    Task AddAsync(Customer customer);
    
    Task UpdateAsync(Customer customer);
    
    Task DeleteAsync(Guid id);
}
