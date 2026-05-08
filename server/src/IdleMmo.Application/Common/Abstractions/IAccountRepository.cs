using IdleMmo.Domain.Entities;

namespace IdleMmo.Application.Common.Abstractions;

public interface IAccountRepository
{
    Task<Account?> FindByGoogleSubAsync(string googleSub, CancellationToken cancellationToken);
    Task<Account?> FindByDeviceHashAsync(string deviceHash, CancellationToken cancellationToken);
    Task<Account?> FindByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
