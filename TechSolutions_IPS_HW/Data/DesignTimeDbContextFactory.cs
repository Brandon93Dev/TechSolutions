using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechSolutions_IPS_HW.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        //Environment loaded as per config setting. if prod we point to prod db, else we point to dev db
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        //Loading the config that we set in the appsettings
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();

        var configuration = configBuilder.Build();

        var preferredConnectionName = env.Equals("Test", StringComparison.OrdinalIgnoreCase)
            ? "TestConnection"
            : "DefaultConnection";

        var connection = configuration.GetConnectionString(preferredConnectionName)
                     ?? configuration[$"ConnectionStrings:{preferredConnectionName}"]
                     ?? configuration.GetConnectionString("DefaultConnection")
                     ?? configuration["ConnectionStrings:DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connection))
            throw new Exception("Connection string not set, please check configuration");           

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connection);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
