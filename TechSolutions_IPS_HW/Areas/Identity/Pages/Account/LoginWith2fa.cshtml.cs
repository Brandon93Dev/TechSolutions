using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TechSolutions_IPS_HW.Areas.Identity.Pages.Account;

public class LoginWith2faModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = null, bool rememberMe = false)
    {
        TempData["StatusMessage"] = "Two-factor authentication is disabled for this application.";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
    }

    public IActionResult OnPost()
    {
        TempData["StatusMessage"] = "Two-factor authentication is disabled for this application.";
        return RedirectToPage("./Login");
    }
}
