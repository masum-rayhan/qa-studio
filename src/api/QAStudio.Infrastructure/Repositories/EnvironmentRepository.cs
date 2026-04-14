using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Data;

namespace QAStudio.Infrastructure.Repositories;

public class EnvironmentRepository : IEnvironmentRepository
{
    private readonly AppDbContext _context;

    public EnvironmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TargetEnvironment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Environments
            .Include(e => e.Creator)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<TargetEnvironment>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Environments
            .Include(e => e.Creator)
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
    }

    public async Task<TargetEnvironment> AddAsync(TargetEnvironment environment, CancellationToken ct = default)
    {
        await _context.Environments.AddAsync(environment, ct);
        await _context.SaveChangesAsync(ct);
        return environment;
    }

    public async Task UpdateAsync(TargetEnvironment environment, CancellationToken ct = default)
    {
        environment.UpdatedAt = DateTime.UtcNow;
        _context.Environments.Update(environment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var environment = await _context.Environments.FindAsync([id], ct);
        if (environment != null)
        {
            _context.Environments.Remove(environment);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Environments.AnyAsync(e => e.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Environments.Where(e => e.Name == name);

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}
