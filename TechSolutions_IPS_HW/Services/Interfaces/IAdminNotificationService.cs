using TechSolutions_IPS_HW.Models.Admin;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface IAdminNotificationService
{
    Task<int> GetUnreadActiveCountAsync(string recipientUserId);
    
    Task<IReadOnlyList<AdminNotification>> GetUnreadActiveAsync(string recipientUserId);
    
    Task<AdminNotification?> GetByIdForRecipientAsync(int id, string recipientUserId);
    
    Task MarkAsReadAsync(AdminNotification notification);
    
    Task MarkAllAsReadBySubjectUserAsync(string subjectUserId);
}
