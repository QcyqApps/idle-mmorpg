using IdleMmo.Application.Common.Abstractions;
using IdleMmo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdleMmo.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly IdleMmoDbContext _db;

    public AccountRepository(IdleMmoDbContext db)
    {
        _db = db;
    }

    public Task<Account?> FindByGoogleSubAsync(string googleSub, CancellationToken cancellationToken) =>
        _db.Accounts.FirstOrDefaultAsync(a => a.GoogleSub == googleSub, cancellationToken);

    public Task<Account?> FindByDeviceHashAsync(string deviceHash, CancellationToken cancellationToken) =>
        _db.Accounts.FirstOrDefaultAsync(a => a.DeviceHash == deviceHash, cancellationToken);

    public Task<Account?> FindByIdAsync(long id, CancellationToken cancellationToken) =>
        _db.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        await _db.Accounts.AddAsync(account, cancellationToken).ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
