namespace TechSolutions_IPS_HW.Models.ViewModels;

/// <summary>
/// View model for displaying employee info in the admin dashboard table.
/// </summary>
public class EmployeeViewModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
