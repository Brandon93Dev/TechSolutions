using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Areas.Identity.Pages.Account.Manage;
 
//Overrides for microsoft's identity pages, we needed to tap into its logic
[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string? Email { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Username must be between {2} and {1} characters.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens.")]
        [Display(Name = "Username")]
        public string DisplayName { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        Email = user.Email;
        Input = new InputModel
        {
            DisplayName = user.DisplayName
        };

        return Page();
    }

    // Post to update exiosting AspUser's profile
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            Email = user.Email;
            return Page();
        }

        user.DisplayName = Input.DisplayName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            Email = user.Email;
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated.";
        return RedirectToPage();
    }
}
