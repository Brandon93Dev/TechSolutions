using Microsoft.AspNetCore.Identity;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Models.ViewModels;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;
using System.Security.Claims;

namespace TechSolutions_IPS_HW.Services.AdminServices;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomerRepository _customerRepository;

    public AdminDashboardService(
        UserManager<ApplicationUser> userManager, 
        ICustomerRepository customerRepository)
    {
        _userManager = userManager;
        _customerRepository = customerRepository;
    }

    //Loads information on employees and limited data on custromers to display on the AdminDashboard
    public async Task<AdminDashboardViewModel> GetDashboardDataAsync(ClaimsPrincipal user)
    {
        var currentUser = await _userManager.GetUserAsync(user);
        var currentDisplayName = currentUser?.DisplayName ?? string.Empty;

        var vm = new AdminDashboardViewModel
        {
            CanViewEmployeeOverview = true
        };

        var employees = await _userManager.GetUsersInRoleAsync("Employee");

        vm.TotalEmployees = employees.Count;
        vm.ApprovedCount = employees.Count(e => e.IsApproved);

        var pendingEmployees = _userManager.Users.Where(a => !a.IsApproved).ToList();
        vm.PendingCount = pendingEmployees.Count;

        //Load approvedc employees
        vm.Employees = employees
            .OrderBy(e => e.DisplayName)
            .Select(e => new EmployeeDashboardItemViewModel
            {
                Id = e.Id,
                DisplayName = e.DisplayName,
                Email = e.Email ?? string.Empty,
                IsApproved = e.IsApproved,
                ApprovedBy = e.ApprovedBy ?? "—",
                ApprovedAt = e.ApprovedAt.HasValue ? 
                e.ApprovedAt.Value.ToString("dd MMM yyyy HH:mm") : "—"
            })
            .ToList();

        //appends the list of unapproved employees and adds it to the list of approved ones
        foreach (var p in pendingEmployees)
        {
            vm.Employees.Add(new EmployeeDashboardItemViewModel
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                Email = p.Email ?? string.Empty,
                IsApproved = false,
                ApprovedBy = "-",
                ApprovedAt = "-"
            });
        }

        vm.Employees = vm.Employees
            .OrderBy(a => a.DisplayName)
            .ThenBy(a => a.ApprovedAt)
            .ToList();      

        //Determines from the list of employees how many are approved by the current context user
        vm.ApprovedByCurrentUserCount = string.IsNullOrEmpty(currentUser.Email) ? 0
            : _userManager.Users.Count(u => u.ApprovedBy == currentUser.Email);

        var allCustomerIds = await _customerRepository.GetAllIdsAsync();
        vm.CustomersByCountry = (
            await _customerRepository
                .GetCountryDistributionByIdsAsync(allCustomerIds, 12))
            .Select(x => new CountryMetricViewModel { Country = x.Country, Count = x.Count })
            .ToList();

        return vm;
    }
}
