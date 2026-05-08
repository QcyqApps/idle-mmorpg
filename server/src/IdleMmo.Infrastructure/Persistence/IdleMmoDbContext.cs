using IdleMmo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdleMmo.Infrastructure.Persistence;

public sealed class IdleMmoDbContext : DbContext
{
    public IdleMmoDbContext(DbContextOptions<IdleMmoDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdleMmoDbContext).Assembly);
    }
}
