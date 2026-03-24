namespace TechSolutions_IPS_HW.Models.ViewModels;

public class EmployeeDashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int DraftCustomers { get; set; }
    public int CustomersCreatedByCurrentUser { get; set; }
    public LastCreatedCustomerViewModel? LastCreatedCustomer { get; set; }
    public List<CountryMetricViewModel> CustomersByCountry { get; set; } = [];
}
