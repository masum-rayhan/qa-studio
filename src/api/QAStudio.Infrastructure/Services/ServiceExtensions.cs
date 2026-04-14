using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QAStudio.Application.Auth.Interfaces;
using QAStudio.Application.Environments.Interfaces;
using QAStudio.Application.TestCases.Interfaces;
using QAStudio.Application.TestRuns.Interfaces;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Configuration;
using QAStudio.Infrastructure.Data;
using QAStudio.Infrastructure.Mappings;
using QAStudio.Infrastructure.Repositories;

namespace QAStudio.Infrastructure.Services;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database — dual provider (SqlServer local, Postgres production)
        var provider = (configuration["Database:Provider"] ?? "SqlServer").Trim();

        if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
        {
            var postgresConn = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "Database:Provider is set to 'Postgres' but ConnectionStrings:Postgres is missing.");

            services.AddDbContext<AppDbContextPostgres>(options =>
                options.UseNpgsql(postgresConn, npgsql =>
                    npgsql.MigrationsAssembly(typeof(AppDbContextPostgres).Assembly.GetName().Name)));

            // Alias AppDbContext to the Postgres context so repositories still use AppDbContext
            services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<AppDbContextPostgres>());
        }
        else if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var sqlServerConn = configuration.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException(
                    "Database:Provider is set to 'SqlServer' but ConnectionStrings:SqlServer is missing.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(sqlServerConn));
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown Database:Provider '{provider}'. Supported: SqlServer, Postgres.");
        }

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
        services.AddScoped<ITestCaseRepository, TestCaseRepository>();
        services.AddScoped<ITestRunRepository, TestRunRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Configuration binding
        var jwtConfig = new JwtConfig();
        configuration.GetSection("Jwt").Bind(jwtConfig);
        services.AddSingleton(jwtConfig);

        var storageConfig = new StorageConfig();
        configuration.GetSection("Storage").Bind(storageConfig);
        services.AddSingleton(storageConfig);

        var playwrightConfig = new PlaywrightConfig();
        configuration.GetSection("Playwright").Bind(playwrightConfig);
        services.AddSingleton(playwrightConfig);

        var notificationsConfig = new NotificationsConfig();
        configuration.GetSection("Notifications").Bind(notificationsConfig);
        services.AddSingleton(notificationsConfig);

        var schedulingConfig = new SchedulingConfig();
        configuration.GetSection("Scheduling").Bind(schedulingConfig);
        services.AddSingleton(schedulingConfig);

        // Services
        services.AddScoped<JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEnvironmentService, EnvironmentService>();
        services.AddScoped<ITestCaseService, TestCaseService>();
        services.AddScoped<ITestRunService, TestRunService>();

        // AutoMapper
        services.AddAutoMapper(typeof(TestCaseMappingProfile).Assembly);

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfig.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
