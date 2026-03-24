using TechSolutions_IPS_HW.Models.Admin;
using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Services.Interfaces;

/// <summary>
/// Notifies all approvers (Admins and Managers) when new users register.
/// Sends email notifications and creates in-app <see cref="AdminNotification"/> records.
/// </summary>
public interface IUserRegistrationNotifier
{
    Task NotifyApproversAsync(
        IReadOnlyList<ApplicationUser> newUsers, 
        CancellationToken cancellationToken = default);
}
