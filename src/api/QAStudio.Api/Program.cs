using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QAStudio.Api.Middleware;
using QAStudio.Application;
using QAStudio.Infrastructure.Data;
using QAStudio.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific local settings (local overrides — NOT committed to git)
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QA Studio API",
        Version = "v1",
        Description = "QA Automation Platform API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register application + infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Allow browser extension to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ExtensionPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Global exception handler — must be first in pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QA Studio API v1"));
}

app.UseHttpsRedirection();
app.UseCors("ExtensionPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
