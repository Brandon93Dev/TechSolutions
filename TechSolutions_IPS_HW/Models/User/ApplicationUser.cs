using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TechSolutions_IPS_HW.Models.Admin;
using TechSolutions_IPS_HW.Models.Audit;

namespace TechSolutions_IPS_HW.Models.User;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string DisplayName { get; set; } = string.Empty;

    // When false the user cannot sign in until an approver sets this to true
    public bool IsApproved { get; set; } = false;

    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovedAt { get; set; }

    // Soft-delete
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAtUtc { get; set; }

    // Navigation properties
    public ICollection<AdminNotification> ReceivedNotifications { get; set; } = new List<AdminNotification>();
    
    public ICollection<AuditActor> AuditActors { get; set; } = new List<AuditActor>();
}
