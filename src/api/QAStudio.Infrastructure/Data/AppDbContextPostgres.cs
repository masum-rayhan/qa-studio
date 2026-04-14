using Microsoft.EntityFrameworkCore;

namespace QAStudio.Infrastructure.Data;

/// <summary>
/// PostgreSQL-specific DbContext. Inherits all entity configuration from
/// <see cref="AppDbContext"/>. Exists solely so EF Core can maintain a
/// separate migration set for PostgreSQL while SQL Server keeps using
/// <see cref="AppDbContext"/>. The active provider is selected at runtime
/// via the <c>Database:Provider</c> app setting.
/// </summary>
public class AppDbContextPostgres : AppDbContext
{
    public AppDbContextPostgres(DbContextOptions<AppDbContextPostgres> options) : base(options)
    {
    }
}
