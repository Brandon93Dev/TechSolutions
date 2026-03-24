using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Audit;
using TechSolutions_IPS_HW.Repositories;

namespace TechSolutions_IPS_HW.Tests.Repositories;

public class AuditRepositoryTests
{
    [Fact]
    public async Task GetCreatedCustomerIdsByPerformerAsync_ReturnsDistinctCustomerIds()
    {
        await using var db = CreateDbContext();

        var performer = new AuditActor { 
            Id = 1, 
            UserId = "employee-1",
            DisplayName = "Employee One", 
            Email = "e1@test.com" 
        };

        var customerId = Guid.NewGuid();

        db.AuditActors.Add(performer);
        db.AuditEntries.AddRange(
            new AuditEntry 
            { 
                Action = "CustomerCreated", 
                PerformerActorId = performer.Id, 
                CustomerId = customerId, 
                TimestampUtc = DateTime.UtcNow.AddMinutes(-5) 
            },
            new AuditEntry 
            { 
                Action = "CustomerCreated",
                PerformerActorId = performer.Id, 
                CustomerId = customerId, 
                TimestampUtc = DateTime.UtcNow 
            });

        await db.SaveChangesAsync();

        var repository = new AuditRepository(db);

        var result = await repository.GetCreatedCustomerIdsByPerformerAsync("employee-1");

        Assert.Single(result);
        Assert.Equal(customerId, result[0]);
    }

    [Fact]
    public async Task GetLastCreatedCustomerIdByPerformerAsync_ReturnsMostRecentCustomer()
    {
        await using var db = CreateDbContext();

        var performer = new AuditActor { 
            Id = 2, 
            UserId = "employee-2",
            DisplayName = "Employee Two",
            Email = "e2@test.com" 
        };

        var olderCustomerId = Guid.NewGuid();
        var newerCustomerId = Guid.NewGuid();

        db.AuditActors.Add(performer);
        db.AuditEntries.AddRange(
            new AuditEntry 
            { 
                Action = "CustomerCreated", 
                PerformerActorId = performer.Id, 
                CustomerId = olderCustomerId, 
                TimestampUtc = DateTime.UtcNow.AddHours(-1) },
            new AuditEntry { 
                Action = "CustomerCreated", 
                PerformerActorId = performer.Id, 
                CustomerId = newerCustomerId, 
                TimestampUtc = DateTime.UtcNow 
            });

        await db.SaveChangesAsync();

        var repository = new AuditRepository(db);

        var result = await repository
            .GetLastCreatedCustomerIdByPerformerAsync("employee-2");

        Assert.Equal(newerCustomerId, result);
    }

    [Fact]
    public async Task GetCustomerCreatorDisplayNamesAsync_ReturnsFirstCreatorPerCustomer()
    {
        await using var db = CreateDbContext();

        var performer1 = new AuditActor 
        { 
            Id = 3, UserId = "employee-3", 
            DisplayName = "First Creator", 
            Email = "e3@test.com" 
        };

        var performer2 = new AuditActor 
        { 
            Id = 4, 
            UserId = "employee-4", 
            DisplayName = "Second Creator", 
            Email = "e4@test.com" 
        };

        var customerId = Guid.NewGuid();

        db.AuditActors.AddRange(performer1, performer2);

        db.AuditEntries.AddRange(
            new AuditEntry 
            { 
                Action = "CustomerCreated", 
                PerformerActorId = performer1.Id, 
                CustomerId = customerId, 
                TimestampUtc = DateTime.UtcNow.AddMinutes(-10) 
            },
            new AuditEntry 
            { 
                Action = "CustomerCreated", 
                PerformerActorId = performer2.Id, 
                CustomerId = customerId, 
                TimestampUtc = DateTime.UtcNow.AddMinutes(-1) 
            });

        await db.SaveChangesAsync();

        var repository = new AuditRepository(db);

        var result = await repository
            .GetCustomerCreatorDisplayNamesAsync([customerId]);

        Assert.True(result.ContainsKey(customerId));
        Assert.Equal("First Creator", result[customerId]);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"audit-repo-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
