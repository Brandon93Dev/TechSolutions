using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Enums;

namespace TechSolutions_IPS_HW.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();
    
    Task<List<Customer>> GetByStatusAsync(CustomerStatus status);
    
    Task<Customer?> GetByIdAsync(Guid id);

    Task<int> CountAsync();

    Task<int> CountByStatusAsync(CustomerStatus status);

    Task<int> CountByIdsAndStatusAsync(IEnumerable<Guid> customerIds, CustomerStatus status);

    Task<List<Guid>> GetAllIdsAsync();

    Task<List<(string Country, int Count)>> GetCountryDistributionByIdsAsync(IEnumerable<Guid> customerIds, int take);
    
    Task<bool> ExistsByIdNumberAsync(string idNumber);

    Task<bool> ExistsByEmailAsync(string email, Guid? excludeCustomerId = null);
    
    Task AddAsync(Customer customer);
    
    Task UpdateAsync(Customer customer);
    
    Task DeleteAsync(Guid id);
}
