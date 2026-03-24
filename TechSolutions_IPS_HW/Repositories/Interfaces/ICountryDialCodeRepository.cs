using TechSolutions_IPS_HW.Models.Reference;

namespace TechSolutions_IPS_HW.Repositories.Interfaces;

public interface ICountryDialCodeRepository
{
    Task<IReadOnlyList<CountryDialCode>> GetAllAsync();
}
