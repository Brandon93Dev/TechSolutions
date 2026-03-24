using TechSolutions_IPS_HW.Models.Admin;

namespace TechSolutions_IPS_HW.Repositories.Interfaces;

/// <summary>
/// Admin notifications <seealso cref="AdminNotification"/> are used to notify 
/// administrators of events related to specific users,
/// </summary>
public interface IAdminNotificationRepository
{
    Task<int> GetUnreadCountAsync(string recipientUserId);
    
    Task<IReadOnlyList<AdminNotification>> GetUnreadAsync(string recipientUserId);
    
    Task<AdminNotification?> GetByIdForRecipientAsync(int id, string recipientUserId);
    
    Task<IReadOnlyList<AdminNotification>> GetUnreadBySubjectUserAsync(
        string subjectUserId, 
        string recipientUserId);
    
    Task MarkAsReadAsync(AdminNotification notification);
    
    Task MarkAsReadAsync(IEnumerable<AdminNotification> notifications);

    Task MarkAllAsReadBySubjectUserAsync(string subjectUserId);

    Task AddRangeAsync(IEnumerable<AdminNotification> notifications, CancellationToken ct = default);
}
