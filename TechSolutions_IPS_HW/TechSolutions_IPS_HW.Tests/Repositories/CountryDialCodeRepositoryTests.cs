using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Reference;
using TechSolutions_IPS_HW.Repositories;

namespace TechSolutions_IPS_HW.Tests.Repositories;

public class CountryDialCodeRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByCountryName()
    {
        await using var db = CreateDbContext();

        db.CountryDialCodes.AddRange(
            new CountryDialCode { 
                CountryName = "Zimbabwe", 
                DialCode = "+263" 
            },
            new CountryDialCode { 
                CountryName = "Algeria", 
                DialCode = "+213" 
            });

        await db.SaveChangesAsync();

        var repository = new CountryDialCodeRepository(db);

        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Algeria", result[0].CountryName);
        Assert.Equal("Zimbabwe", result[1].CountryName);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"country-dial-repo-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
