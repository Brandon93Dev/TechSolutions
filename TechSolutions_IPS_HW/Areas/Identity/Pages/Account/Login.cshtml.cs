using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }


    //Override on Microsoft's base login page , we needed to plug in to add some additional logic here
    //including lock out system depending on if user supplies the incorrect password too many times in
    //a small period of time, this is to ensure we have bruteforce protection on the site.
    //We also introduced funcitonaltiy to check if a user's Registration was approved by a manager or administrator
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            // Check if account is already locked out before attempting sign-in
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null && await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Locked-out user {Email} attempted to log in.", Input.Email);
                ModelState.AddModelError(string.Empty,
                    "Your account has been locked due to multiple failed login attempts. " +
                    "Please contact support@techsystems.co.za for assistance.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }     
            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Unapproved user {Email} provided valid credentials.", Input.Email);
                ModelState.AddModelError(string.Empty,
                    "Your account has not been approved yet. " +
                    "Please wait for an administrator to approve your registration.");
                return Page();
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty,
                    "Your account has been locked due to multiple failed login attempts. " +
                    "Please contact support@techsystems.co.za for assistance.");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
