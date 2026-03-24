using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Reference;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Repositories;

public class CountryDialCodeRepository : ICountryDialCodeRepository
{
    private readonly ApplicationDbContext _db;

    public CountryDialCodeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    //Get list of countries and their associated dial codes, ordered by country name
    public async Task<IReadOnlyList<CountryDialCode>> GetAllAsync()
    {
        return await _db.CountryDialCodes
            .AsNoTracking()
            .OrderBy(c => c.CountryName)
            .ToListAsync();
    }
}
