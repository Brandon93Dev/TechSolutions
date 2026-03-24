using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Admin;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Repositories;

/// <summary>
/// Implementation of <see cref="IAdminNotificationRepository"/>
/// </summary>
public class AdminNotificationRepository : IAdminNotificationRepository
{
    private readonly ApplicationDbContext _db;

    public AdminNotificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetUnreadCountAsync(string recipientUserId)
    {
        return await _db.AdminNotifications
            .CountAsync(n => n.RecipientUserId == recipientUserId && !n.IsRead);
    }

    public async Task<IReadOnlyList<AdminNotification>> GetUnreadAsync(string recipientUserId)
    {
        return await _db.AdminNotifications
            .Where(n => n.RecipientUserId == recipientUserId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<AdminNotification?> GetByIdForRecipientAsync(int id, string recipientUserId)
    {
        return await _db.AdminNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == recipientUserId);
    }

    public async Task<IReadOnlyList<AdminNotification>> 
        GetUnreadBySubjectUserAsync(
        string subjectUserId, 
        string recipientUserId)
    {
        return await _db.AdminNotifications
            .Where(n => n.SubjectUserId == subjectUserId && n.RecipientUserId == recipientUserId && !n.IsRead)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(AdminNotification notification)
    {
        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(IEnumerable<AdminNotification> notifications)
    {
        foreach (var n in notifications) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadBySubjectUserAsync(string subjectUserId)
    {
        var notifications = await _db.AdminNotifications
            .Where(n => n.SubjectUserId == subjectUserId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(
        IEnumerable<AdminNotification> notifications, 
        CancellationToken ct = default)
    {
        _db.AdminNotifications.AddRange(notifications);
        await _db.SaveChangesAsync(ct);
    }
}
