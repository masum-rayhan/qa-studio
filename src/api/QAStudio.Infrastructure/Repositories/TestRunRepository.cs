using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Data;

namespace QAStudio.Infrastructure.Repositories;

public class TestRunRepository : ITestRunRepository
{
    private readonly AppDbContext _context;

    public TestRunRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TestRun?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TestRuns
            .Include(tr => tr.TestCase)
            .Include(tr => tr.Environment)
            .Include(tr => tr.TriggeredByUser)
            .Include(tr => tr.Results.OrderBy(r => r.StepIndex))
            .FirstOrDefaultAsync(tr => tr.Id == id, ct);
    }

    public async Task<IReadOnlyList<TestRun>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.TestRuns
            .Include(tr => tr.TestCase)
            .Include(tr => tr.Environment)
            .Include(tr => tr.TriggeredByUser)
            .OrderByDescending(tr => tr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TestRun>> GetByTestCaseAsync(Guid testCaseId, CancellationToken ct = default)
    {
        return await _context.TestRuns
            .Include(tr => tr.TestCase)
            .Include(tr => tr.Environment)
            .Include(tr => tr.TriggeredByUser)
            .Where(tr => tr.TestCaseId == testCaseId)
            .OrderByDescending(tr => tr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<TestRun> AddAsync(TestRun testRun, CancellationToken ct = default)
    {
        await _context.TestRuns.AddAsync(testRun, ct);
        await _context.SaveChangesAsync(ct);
        return testRun;
    }

    public async Task UpdateAsync(TestRun testRun, CancellationToken ct = default)
    {
        testRun.UpdatedAt = DateTime.UtcNow;
        _context.TestRuns.Update(testRun);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddResultAsync(TestResult result, CancellationToken ct = default)
    {
        await _context.TestResults.AddAsync(result, ct);
        await _context.SaveChangesAsync(ct);
    }
}
