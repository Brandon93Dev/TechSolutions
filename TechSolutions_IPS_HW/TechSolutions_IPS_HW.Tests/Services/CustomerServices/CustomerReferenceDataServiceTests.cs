using Moq;
using TechSolutions_IPS_HW.Models.Reference;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.CustomerServices;

namespace TechSolutions_IPS_HW.Tests.Services.CustomerServices;

public class CustomerReferenceDataServiceTests
{
    [Fact]
    public async Task GetCountryDialCodesAsync_DelegatesToRepository()
    {
        var expected = (IReadOnlyList<CountryDialCode>)
        [
            new CountryDialCode { 
                CountryName = "South Africa", 
                DialCode = "+27" 
            },
            new CountryDialCode { 
                CountryName = "United Kingdom", 
                DialCode = "+44" 
            }
        ];

        var repository = new Mock<ICountryDialCodeRepository>();
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

        var sut = new CustomerReferenceDataService(repository.Object);

        var result = await sut.GetCountryDialCodesAsync();

        Assert.Same(expected, result);
    }
}
