using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _userStore = userStore;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    //Expanded on the registration model to ensure we can store username
    public class InputModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Username must be between {2} and {1} characters.", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens.")]
        [Display(Name = "Username")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                DisplayName = Input.DisplayName
            };
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);

            // Set email via the IUserEmailStore
            if (_userStore is IUserEmailStore<ApplicationUser> emailStore)
            {
                await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            }

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                return RedirectToPage("./Login");
            }

            // Hide account-existence errors to prevent username enumeration
            foreach (var error in result.Errors)
            {
                if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                {
                    _logger.LogWarning("Registration attempt for existing account: {Email}", Input.Email);
                    return RedirectToPage("./Login");
                }

                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
