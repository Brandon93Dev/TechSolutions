using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Controllers;

[Authorize(Roles = "Administrator,Management",Policy = "CanApproveUsers")]
[Route("admin/dashboard")]
public class AdminDashboardController : Controller
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }   

    [HttpGet("")]
    public IActionResult Index() => View();

    //Fetch admin dashboard data
    [HttpGet("data")]
    public async Task<IActionResult> GetDashboardData()
    {
        var data = await _dashboardService.GetDashboardDataAsync(User);
        return Json(data);
    }
}
