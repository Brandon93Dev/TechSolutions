using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Controllers;

[Authorize(Roles = "Employee")]
[Route("employee/dashboard")]
public class EmployeeDashboardController : Controller
{
    private readonly IEmployeeDashboardService _dashboardService;

    public EmployeeDashboardController(IEmployeeDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("")]
    public IActionResult Index() => View();

    //Load customer related infor for the employee dashboard
    [HttpGet("data")]
    public async Task<IActionResult> GetDashboardData()
    {
        var data = await _dashboardService.GetDashboardDataAsync(User);
        return Json(data);
    }
}
