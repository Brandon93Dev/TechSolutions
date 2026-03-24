using TechSolutions_IPS_HW.Models.Reference;

namespace TechSolutions_IPS_HW.Services.Interfaces;

public interface ICustomerReferenceDataService
{
    Task<IReadOnlyList<CountryDialCode>> GetCountryDialCodesAsync();
}
