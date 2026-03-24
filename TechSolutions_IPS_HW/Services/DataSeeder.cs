using Microsoft.AspNetCore.Identity;
using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Services;

public static class DataSeeder
{

    // is just called initially or if database gets ruined, used to creat the initial
    //admin and manager accounts, cant register users if there isnt anyoen to approve them
    public static async Task SeedInitialUsersAsync(
        WebApplication app, string adminEmail, string adminPassword, string managerEmail,
        string managerPassword, string[] approverRoles)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        //Userna dn role manager created to sepecrate Admins, Managers and Employees
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure required roles exist (from configuration) with some default fallbacks
        var rolesToEnsure = approverRoles?.
            Where(r => 
                !string.IsNullOrWhiteSpace(r)).ToArray() ?? new[] { 
                    "Administrator", 
                    "Management" 
                };

        // Employee role is for all other users approved by Admin or Manager
        var allRoles = rolesToEnsure.
            Append("Employee").
            Distinct(StringComparer.OrdinalIgnoreCase).
            ToArray();

        foreach (var r in allRoles)
        {
            if (!await roleManager.RoleExistsAsync(r))                
                await roleManager.CreateAsync(new IdentityRole(r));                
        }

        #region Setup Admin
        //Setup admin
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Admin",
                EmailConfirmed = true,
                IsApproved = true,
                ApprovedAt = DateTime.UtcNow,
                ApprovedBy = "system-seed"
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {                  
                var primaryRole = rolesToEnsure.FirstOrDefault() ?? "Administrator";
                await userManager.AddToRoleAsync(admin, primaryRole);
            }
        }
        #endregion

        #region Setup Manager
        var manager = await userManager.FindByEmailAsync(managerEmail);
        if(manager == null)
        {
            manager = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                DisplayName = "Manager",
                EmailConfirmed = true,
                IsApproved = true,
                ApprovedAt = DateTime.UtcNow,
                ApprovedBy = "system-seed"
            };

            var result = await userManager.CreateAsync(manager, managerPassword);
            if (result.Succeeded)
            {
                var secondaryRole = rolesToEnsure.Skip(1).FirstOrDefault() ?? "Management";
                await userManager.AddToRoleAsync(manager, secondaryRole);
            }
        }
        #endregion
    }
}
