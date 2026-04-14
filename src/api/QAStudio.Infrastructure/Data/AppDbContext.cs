using Microsoft.EntityFrameworkCore;
using QAStudio.Domain.Entities;

namespace QAStudio.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TargetEnvironment> Environments => Set<TargetEnvironment>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<TestRun> TestRuns => Set<TestRun>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<TestSchedule> TestSchedules => Set<TestSchedule>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>();
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TargetEnvironment
        modelBuilder.Entity<TargetEnvironment>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedEnvironments)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TestCase
        modelBuilder.Entity<TestCase>(entity =>
        {
            entity.HasIndex(tc => tc.Name);
            entity.HasOne(tc => tc.TargetEnvironment)
                  .WithMany(e => e.TestCases)
                  .HasForeignKey(tc => tc.TargetEnvironmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(tc => tc.Creator)
                  .WithMany(u => u.TestCases)
                  .HasForeignKey(tc => tc.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TestRun
        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.HasIndex(tr => tr.Status);
            entity.HasIndex(tr => tr.CreatedAt);
            entity.Property(tr => tr.Status).HasConversion<string>();
            entity.HasOne(tr => tr.TestCase)
                  .WithMany(tc => tc.TestRuns)
                  .HasForeignKey(tr => tr.TestCaseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tr => tr.Environment)
                  .WithMany(e => e.TestRuns)
                  .HasForeignKey(tr => tr.EnvironmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(tr => tr.TriggeredByUser)
                  .WithMany(u => u.TriggeredRuns)
                  .HasForeignKey(tr => tr.TriggeredBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TestResult
        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.Property(r => r.Action).HasConversion<string>();
            entity.Property(r => r.Status).HasConversion<string>();
            entity.HasOne(r => r.TestRun)
                  .WithMany(tr => tr.Results)
                  .HasForeignKey(r => r.TestRunId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TestSchedule
        modelBuilder.Entity<TestSchedule>(entity =>
        {
            entity.HasOne(s => s.TestCase)
                  .WithMany(tc => tc.TestSchedules)
                  .HasForeignKey(s => s.TestCaseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Environment)
                  .WithMany(e => e.TestSchedules)
                  .HasForeignKey(s => s.EnvironmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Creator)
                  .WithMany(u => u.Schedules)
                  .HasForeignKey(s => s.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
