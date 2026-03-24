using Microsoft.AspNetCore.Identity;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Models.ViewModels;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;
using System.Security.Claims;

namespace TechSolutions_IPS_HW.Services.AdminServices;

public class EmployeeDashboardService : IEmployeeDashboardService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditRepository _auditRepository;

    public EmployeeDashboardService(
        UserManager<ApplicationUser> userManager,
        ICustomerRepository customerRepository,
        IAuditRepository auditRepository)
    {
        _userManager = userManager;
        _customerRepository = customerRepository;
        _auditRepository = auditRepository;
    }

    //Loads information on customers to display on the employee dashboard, incl total customers, active/draft customers,
    //customers created by the current context user, last created customer details and distribution of customers by country
    public async Task<EmployeeDashboardViewModel> GetDashboardDataAsync(ClaimsPrincipal user)
    {
        var currentUser = await _userManager.GetUserAsync(user);
        var currentUserId = currentUser?.Id;

        var vm = new EmployeeDashboardViewModel
        {
            TotalCustomers = await _customerRepository.CountAsync()
        };

        var customerScopeIds = string.IsNullOrWhiteSpace(currentUserId)
            ? []
            : await _auditRepository.GetCreatedCustomerIdsByPerformerAsync(currentUserId);

        vm.ActiveCustomers = await _customerRepository
            .CountByIdsAndStatusAsync(customerScopeIds, CustomerStatus.Active);
        vm.DraftCustomers = await _customerRepository
            .CountByIdsAndStatusAsync(customerScopeIds, CustomerStatus.Draft);
        vm.CustomersCreatedByCurrentUser = customerScopeIds.Count;

        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            var lastCustomerId = await _auditRepository
                .GetLastCreatedCustomerIdByPerformerAsync(currentUserId);
            if (lastCustomerId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(lastCustomerId.Value);
                if (customer != null)
                {
                    var location = !string.IsNullOrWhiteSpace(customer.AddressCountry)
                        ? customer.AddressCountry
                        : (!string.IsNullOrWhiteSpace(customer.Nationality) ? customer.Nationality : "Unspecified");

                    //loads information on the latest created customer by this context employee
                    vm.LastCreatedCustomer = new LastCreatedCustomerViewModel
                    {
                        CustomerId = customer.CustomerId,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        Status = customer.Status.ToString(),
                        Location = location,
                        CreatedAt = customer.CreatedAt.ToString("dd MMM yyyy HH:mm")
                    };
                }
            }
        }

        vm.CustomersByCountry = 
            (await _customerRepository
                .GetCountryDistributionByIdsAsync(customerScopeIds, 12))
            .Select(x => new CountryMetricViewModel { Country = x.Country, Count = x.Count })
            .ToList();

        return vm;
    }
}
