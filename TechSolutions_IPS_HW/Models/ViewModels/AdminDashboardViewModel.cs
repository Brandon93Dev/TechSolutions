namespace TechSolutions_IPS_HW.Models.ViewModels;

/// <summary>
/// Top-level view model for the Admin Dashboard page.
/// Holds the employee list and a placeholder metrics section for future use.
/// </summary>
public class AdminDashboardViewModel
{    
    public bool CanViewEmployeeOverview { get; set; }

    public List<EmployeeDashboardItemViewModel> Employees { get; set; } = [];

    #region Metrics
    public int TotalEmployees { get; set; }
    
    public int ApprovedCount { get; set; }
    
    public int PendingCount { get; set; }

    public int ApprovedByCurrentUserCount { get; set; }

    public List<CountryMetricViewModel> CustomersByCountry { get; set; } = [];
    #endregion
}

public class EmployeeDashboardItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string ApprovedBy { get; set; } = "—";
    public string ApprovedAt { get; set; } = "—";
}

public class LastCreatedCustomerViewModel
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class CountryMetricViewModel
{
    public string Country { get; set; } = string.Empty;
    public int Count { get; set; }
}
