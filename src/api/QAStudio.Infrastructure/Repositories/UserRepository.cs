using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Data;

namespace QAStudio.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Id == id, ct);

    public override async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        user.Email = user.Email.ToLower();
        return await base.AddAsync(user, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet.AnyAsync(u => u.Email == email.ToLower(), ct);
}
