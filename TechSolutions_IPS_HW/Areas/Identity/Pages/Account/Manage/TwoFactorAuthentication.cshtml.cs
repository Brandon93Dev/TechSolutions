using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TechSolutions_IPS_HW.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel : PageModel
{

    //Overrides on microsoft's identity pages and logic
    public IActionResult OnGet()
    {
        TempData["StatusMessage"] = "Two-factor authentication is disabled for this application.";
        return RedirectToPage("./Index");
    }
}
