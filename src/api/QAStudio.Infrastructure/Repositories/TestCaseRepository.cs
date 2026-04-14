using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Data;

namespace QAStudio.Infrastructure.Repositories;

public class TestCaseRepository : ITestCaseRepository
{
    private readonly AppDbContext _context;

    public TestCaseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TestCase?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TestCases
            .Include(tc => tc.TargetEnvironment)
            .Include(tc => tc.Creator)
            .FirstOrDefaultAsync(tc => tc.Id == id, ct);
    }

    public async Task<IReadOnlyList<TestCase>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.TestCases
            .Include(tc => tc.TargetEnvironment)
            .Include(tc => tc.Creator)
            .OrderByDescending(tc => tc.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TestCase>> GetByEnvironmentAsync(Guid environmentId, CancellationToken ct = default)
    {
        return await _context.TestCases
            .Include(tc => tc.TargetEnvironment)
            .Include(tc => tc.Creator)
            .Where(tc => tc.TargetEnvironmentId == environmentId)
            .OrderByDescending(tc => tc.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<TestCase> AddAsync(TestCase testCase, CancellationToken ct = default)
    {
        await _context.TestCases.AddAsync(testCase, ct);
        await _context.SaveChangesAsync(ct);
        return testCase;
    }

    public async Task UpdateAsync(TestCase testCase, CancellationToken ct = default)
    {
        testCase.UpdatedAt = DateTime.UtcNow;
        _context.TestCases.Update(testCase);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var testCase = await _context.TestCases.FindAsync([id], ct);
        if (testCase != null)
        {
            _context.TestCases.Remove(testCase);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TestCases.AnyAsync(tc => tc.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.TestCases.Where(tc => tc.Name == name);

        if (excludeId.HasValue)
            query = query.Where(tc => tc.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}
