using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Services.AuthenticationServices;

public class AppSignInManagerService : SignInManager<ApplicationUser>
{

    //Piggybacked off of what microsoft's identiy provider provides
    public AppSignInManagerService(UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, 
            optionsAccessor, logger, schemes, confirmation)
    {
    }

    // Overriding Identity providers PasswordSignInAsync method to check if the user is approved
    // before allowing them to sign in.
    // If the password is correct but the account isn't approved yet, we return NotAllowed
    // WITHOUT counting it as a failed attempt (no lockout increment for valid credentials).
    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var user = await UserManager.FindByNameAsync(userName);
        if (user == null)
            return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

        // If the user IS approved, just let Identity handle everything normally
        if (user.IsApproved)
            return await base.PasswordSignInAsync(
                userName, 
                password, 
                isPersistent, 
                lockoutOnFailure);

        // User is NOT approved — verify the password first so we don't reveal account existence
        // this is often a mistake made by developers, its easy to see if a user account exists by
        // attempting differnt account names (even wioth incorrewct passwords) if a risky response
        // is sent back the user wiill then be able to see, oh this account exists... just the
        // wrong password, that allows the malicious actor to focus on brute force attacks on that
        // account.
        var passwordValid = await UserManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            // Wrong password on an unapproved account — still count toward lockout
            if (lockoutOnFailure && await UserManager.GetLockoutEnabledAsync(user))
            {
                await UserManager.AccessFailedAsync(user);
            }
            return SignInResult.Failed;
        }

        // Correct password but not approved — reset failed count (don't punish valid credentials)
        // and tell the caller the account isn't allowed yet
        await UserManager.ResetAccessFailedCountAsync(user);
        return SignInResult.NotAllowed;
    }
}
