using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechSolutions_IPS_HW.Models;

namespace TechSolutions_IPS_HW.Controllers;

public class HomeController : Controller
{
    //Override home page depending on what user is authenticated, we redirect managers and admins to 1 page and 
    //redirect employees to another
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Administrator") || User.IsInRole("Management"))
                return RedirectToAction("Index", "AdminDashboard");

            if (User.IsInRole("Employee"))
                return RedirectToAction("Index", "EmployeeDashboard");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { 
                RequestId = Activity.Current?.Id ?? 
                HttpContext.TraceIdentifier 
            });
    }
}
