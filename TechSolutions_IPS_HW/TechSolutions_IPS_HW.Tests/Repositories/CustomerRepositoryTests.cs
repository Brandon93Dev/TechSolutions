using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories;

namespace TechSolutions_IPS_HW.Tests.Repositories;

public class CustomerRepositoryTests
{
    [Fact]
    public async Task CountByIdsAndStatusAsync_ReturnsZero_WhenNoIdsProvided()
    {
        await using var db = CreateDbContext();
        var repository = new CustomerRepository(db);

        var result = await repository.CountByIdsAndStatusAsync([], CustomerStatus.Active);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCountryDistributionByIdsAsync_UsesAddressCountryThenNationalityFallback()
    {
        await using var db = CreateDbContext();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        db.Customers.AddRange(
            new Customer
            {
                CustomerId = firstId,
                FirstName = "A",
                Surname = "One",
                Email = "a1@test.com",
                AddressCountry = "South Africa"
            },
            new Customer
            {
                CustomerId = secondId,
                FirstName = "B",
                Surname = "Two",
                Email = "b2@test.com",
                Nationality = "Namibia"
            });

        await db.SaveChangesAsync();

        var repository = new CustomerRepository(db);

        var result = await repository.GetCountryDistributionByIdsAsync([firstId, secondId], 10);

        Assert.Contains(result, r => r.Country == "South Africa" && r.Count == 1);
        Assert.Contains(result, r => r.Country == "Namibia" && r.Count == 1);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesCustomer()
    {
        await using var db = CreateDbContext();
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "A",
            Surname = "One",
            Email = "delete@test.com"
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var repository = new CustomerRepository(db);

        await repository.DeleteAsync(customer.CustomerId);

        var deleted = await db.Customers
            .IgnoreQueryFilters()
            .SingleAsync(c => c.CustomerId == customer.CustomerId);

        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAtUtc);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"customer-repo-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
