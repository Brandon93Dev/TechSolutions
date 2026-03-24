using System.Security.Claims;
using TechSolutions_IPS_HW.Models.ViewModels;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface IEmployeeDashboardService
{
    Task<EmployeeDashboardViewModel> GetDashboardDataAsync(ClaimsPrincipal user);
}
