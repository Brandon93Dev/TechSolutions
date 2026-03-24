using Microsoft.AspNetCore.Identity;
using TechSolutions_IPS_HW.Models.Admin;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.NotificationServices;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly IAdminNotificationRepository _notificationRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminNotificationService(
        IAdminNotificationRepository notificationRepository, 
        UserManager<ApplicationUser> userManager)
    {
        _notificationRepository = notificationRepository;
        _userManager = userManager;
    }
    
    public async Task<int> GetUnreadActiveCountAsync(string recipientUserId)
    {
        var active = await GetUnreadActiveAsync(recipientUserId);
        return active.Count;
    }

    //Gets list of unread or unactioned notifications to display to all managers and admins
    public async Task<IReadOnlyList<AdminNotification>> GetUnreadActiveAsync(
        string recipientUserId)
    {
        var notifications = await _notificationRepository.
            GetUnreadAsync(recipientUserId);

        var stale = new List<AdminNotification>();
        var active = new List<AdminNotification>();

        foreach (var n in notifications)
        {
            if (string.IsNullOrEmpty(n.SubjectUserId))
            {
                active.Add(n);
                continue;
            }

            var subjectUser = await _userManager.FindByIdAsync(n.SubjectUserId);
            if (subjectUser == null || subjectUser.IsApproved)
                stale.Add(n);
            else
                active.Add(n);
        }

        if (stale.Count > 0)
            await _notificationRepository.MarkAsReadAsync(stale);

        return active;
    }

    public Task<AdminNotification?> GetByIdForRecipientAsync(int id, string recipientUserId)
        => _notificationRepository.GetByIdForRecipientAsync(id, recipientUserId);

    public Task MarkAsReadAsync(AdminNotification notification)
        => _notificationRepository.MarkAsReadAsync(notification);

    public Task MarkAllAsReadBySubjectUserAsync(string subjectUserId)
        => _notificationRepository.MarkAllAsReadBySubjectUserAsync(subjectUserId);
}
