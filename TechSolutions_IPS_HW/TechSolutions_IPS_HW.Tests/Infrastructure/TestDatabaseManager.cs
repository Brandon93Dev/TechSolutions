using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.Customer;

namespace TechSolutions_IPS_HW.Tests.Infrastructure;

public static class TestDatabaseManager
{
    public static async Task<ApplicationDbContext> CreateMigratedContextAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("TestConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Test database connection string is not configured.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var db = new ApplicationDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }

    public static async Task SeedCustomersAsync(ApplicationDbContext db, IEnumerable<Customer> customers)
    {
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();
    }
}
