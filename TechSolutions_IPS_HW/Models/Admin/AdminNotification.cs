using TechSolutions_IPS_HW.Models.User;

namespace TechSolutions_IPS_HW.Models.Admin;

public class AdminNotification
{
    public int Id { get; set; }

    /// <summary>
    /// The admin/manager user who should see this notification.
    /// </summary>
    public string RecipientUserId { get; set; } = string.Empty;

    public ApplicationUser? Recipient { get; set; }

    /// <summary>
    /// The newly registered user this notification is about
    /// </summary>
    public string? SubjectUserId { get; set; }
    
    public ApplicationUser? SubjectUser { get; set; }

    /// <summary>
    /// Email of the newly registered user – for display purposes.
    /// </summary>
    public string? SubjectUserEmail { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
