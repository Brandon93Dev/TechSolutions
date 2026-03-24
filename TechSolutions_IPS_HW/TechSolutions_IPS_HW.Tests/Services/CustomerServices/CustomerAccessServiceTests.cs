using System.Security.Claims;
using Moq;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Services.CustomerServices;

namespace TechSolutions_IPS_HW.Tests.Services.CustomerServices;

public class CustomerAccessServiceTests
{
    private readonly Mock<IAuditRepository> _auditRepository = new();

    /// <summary>
    /// Creates a new instance of the CustomerAccessService using the configured audit repository mock.
    /// </summary>
    /// <remarks>This method is typically used in unit tests to provide a testable instance of
    /// CustomerAccessService with mocked dependencies.</remarks>
    /// <returns>A CustomerAccessService instance initialized with the current mock audit repository.</returns>
    private CustomerAccessService CreateSut() => new(_auditRepository.Object);

    [Fact]
    public void IsPrivilegedUser_ReturnsTrue_ForAdministrator()
    {
        var sut = CreateSut();
        var user = CreateUser("user-1", "Administrator");

        var result = sut.IsPrivilegedUser(user);

        Assert.True(result);
    }

    [Fact]
    public async Task IsEmployeeBlockedFromCustomerAsync_ReturnsFalse_ForNonEmployee()
    {
        var sut = CreateSut();
        var user = CreateUser("user-1", "Management");

        var result = await sut.IsEmployeeBlockedFromCustomerAsync(user, Guid.NewGuid());

        Assert.False(result);
        _auditRepository.Verify(r => r.HasCreatedCustomerAsync(
            It.IsAny<Guid>(), 
            It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public async Task IsEmployeeBlockedFromCustomerAsync_ReturnsTrue_WhenEmployeeHasNoUserId()
    {
        var sut = CreateSut();

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, "Employee")], 
            "test");

        var user = new ClaimsPrincipal(identity);

        var result = await sut.IsEmployeeBlockedFromCustomerAsync(
            user, 
            Guid.NewGuid());

        Assert.True(result);
    }

    [Fact]
    public async Task IsEmployeeBlockedFromCustomerAsync_ReturnsFalse_WhenEmployeeCreatedCustomer()
    {
        var customerId = Guid.NewGuid();
        _auditRepository.Setup(r => r.HasCreatedCustomerAsync(
                customerId, 
                "user-1"))
            .ReturnsAsync(true);

        var sut = CreateSut();
        var user = CreateUser("user-1", "Employee");

        var result = await sut.IsEmployeeBlockedFromCustomerAsync(
            user, 
            customerId);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllowedCustomerIdsAsync_ReturnsMappedIds_ForEmployee()
    {
        var customerId = Guid.NewGuid();
        _auditRepository.Setup(r => r.GetCreatedCustomerIdsByPerformerAsync("user-1"))
            .ReturnsAsync([customerId, customerId]);

        var sut = CreateSut();
        var user = CreateUser("user-1", "Employee");

        var result = await sut.GetAllowedCustomerIdsAsync(user);

        Assert.Single(result);
        Assert.Contains(customerId, result);
    }

    private static ClaimsPrincipal CreateUser(string? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(userId))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
