using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QAStudio.Infrastructure.Data;

/// <summary>
/// Design-time factory for <see cref="AppDbContext"/> (the SQL Server context).
/// Exists so <c>dotnet ef</c> tooling can build the SQL Server context without
/// running full app startup.
/// </summary>
public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Development.local.json", optional: true)
            .AddEnvironmentVariables();

        // Look for appsettings in the API project folder (one level up from Infrastructure)
        var apiPath = Path.Combine(basePath, "QAStudio.Api");
        if (Directory.Exists(apiPath))
        {
            configBuilder
                .AddJsonFile(Path.Combine(apiPath, "appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine(apiPath, "appsettings.Development.json"), optional: true)
                .AddJsonFile(Path.Combine(apiPath, "appsettings.Development.local.json"), optional: true);
        }

        var config = configBuilder.Build();

        var connectionString = config.GetConnectionString("SqlServer")
            ?? "Server=localhost;Database=QAStudioDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
