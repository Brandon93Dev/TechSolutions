using Moq;
using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.CustomerServices;

namespace TechSolutions_IPS_HW.Tests.Services.CustomerServices;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IAuditRepository> _auditRepository = new();

    private CustomerService CreateSut() => new(_customerRepository.Object, _auditRepository.Object);

    [Fact]
    public async Task GetCustomersAsync_UsesStatusFilter_WhenStatusIsValid()
    {
        var expected = new List<Customer> { new() { 
            FirstName = "A", 
            Surname = "B", 
            Email = "a@b.com" } 
        };

        _customerRepository.Setup(r => r.GetByStatusAsync(CustomerStatus.Active))
            .ReturnsAsync(expected);

        var sut = CreateSut();
        var result = await sut.GetCustomersAsync("active");

        Assert.Same(expected, result);
        _customerRepository.Verify(r => 
            r.GetByStatusAsync(CustomerStatus.Active), 
            Times.Once);

        _customerRepository.Verify(r =>
            r.GetAllAsync(), 
            Times.Never);
    }

    [Fact]
    public async Task GetCustomersAsync_UsesGetAll_WhenStatusIsInvalid()
    {
        var expected = new List<Customer> { 
            new() { 
                FirstName = "A", 
                Surname = "B",
                Email = "a@b.com" 
            }
        };
        _customerRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

        var sut = CreateSut();
        var result = await sut.GetCustomersAsync("not-a-status");

        Assert.Same(expected, result);
        _customerRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _customerRepository.Verify(r => r.GetByStatusAsync(It.IsAny<CustomerStatus>()), Times.Never);
    }

    [Fact]
    public async Task ExistsByEmailAsync_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        _customerRepository.Setup(r => r.ExistsByEmailAsync("email@x.com", id)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ExistsByEmailAsync("email@x.com", id);

        Assert.True(result);
        _customerRepository.Verify(r => r.ExistsByEmailAsync("email@x.com", id), Times.Once);
    }

    [Fact]
    public async Task GetCustomerCreatorDisplayNamesAsync_DelegatesToAuditRepository()
    {
        var customerId = Guid.NewGuid();
        var expected = new Dictionary<Guid, string> { [customerId] = "Creator User" };
        _auditRepository.Setup(r => 
            r.GetCustomerCreatorDisplayNamesAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();
        var result = await sut.GetCustomerCreatorDisplayNamesAsync(new[] { customerId });

        Assert.Same(expected, result);
        _auditRepository.Verify(r => r.GetCustomerCreatorDisplayNamesAsync(
            It.Is<IEnumerable<Guid>>(x => x.Contains(customerId))), 
            Times.Once);
    }

    [Fact]
    public async Task AddAsync_DelegatesToRepository()
    {
        var customer = new Customer { 
            FirstName = "A", 
            Surname = "B", 
            Email = "a@b.com" 
        };

        var sut = CreateSut();

        await sut.AddAsync(customer);

        _customerRepository.Verify(r => 
            r.AddAsync(customer), 
            Times.Once);
    }
}
