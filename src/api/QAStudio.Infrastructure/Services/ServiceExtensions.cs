using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QAStudio.Application.Auth.Interfaces;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Configuration;
using QAStudio.Infrastructure.Data;
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

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConn, npgsql =>
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));
        }
        else
        {
            var sqlServerConn = configuration.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException(
                    "Database:Provider is set to 'SqlServer' but ConnectionStrings:SqlServer is missing.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(sqlServerConn));
        }

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
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
