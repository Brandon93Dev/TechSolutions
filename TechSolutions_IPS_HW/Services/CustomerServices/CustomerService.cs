using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.CustomerServices;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditRepository _auditRepository;

    public CustomerService(
        ICustomerRepository customerRepository, 
        IAuditRepository auditRepository)
    {
        _customerRepository = customerRepository;
        _auditRepository = auditRepository;
    }

    public async Task<List<Customer>> GetCustomersAsync(string? status)
    {
        if (Enum.TryParse<CustomerStatus>(status, true, out var parsed))
            return await _customerRepository.GetByStatusAsync(parsed);

        return await _customerRepository.GetAllAsync();
    }

    public Task<Customer?> GetByIdAsync(Guid id)
        => _customerRepository.GetByIdAsync(id);

    public Task<Dictionary<Guid, string>> GetCustomerCreatorDisplayNamesAsync(
        IEnumerable<Guid> customerIds)
        => _auditRepository.GetCustomerCreatorDisplayNamesAsync(customerIds);

    public Task<bool> ExistsByEmailAsync(
        string email, Guid? excludeCustomerId = null)
        => _customerRepository.ExistsByEmailAsync(email, excludeCustomerId);

    public Task<bool> ExistsByIdNumberAsync(string idNumber)
        => _customerRepository.ExistsByIdNumberAsync(idNumber);

    public Task AddAsync(Customer customer)
        => _customerRepository.AddAsync(customer);

    public Task UpdateAsync(Customer customer)
        => _customerRepository.UpdateAsync(customer);

    public Task DeleteAsync(Guid id)
        => _customerRepository.DeleteAsync(id);
}
