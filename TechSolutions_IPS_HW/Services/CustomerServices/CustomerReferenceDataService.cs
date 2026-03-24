using TechSolutions_IPS_HW.Models.Reference;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Services.CustomerServices;

public class CustomerReferenceDataService : ICustomerReferenceDataService
{
    private readonly ICountryDialCodeRepository _countryDialCodeRepository;

    public CustomerReferenceDataService(
        ICountryDialCodeRepository countryDialCodeRepository)
    {
        _countryDialCodeRepository = countryDialCodeRepository;
    }

    public Task<IReadOnlyList<CountryDialCode>> GetCountryDialCodesAsync()
    {
        return _countryDialCodeRepository.GetAllAsync();
    }
}
