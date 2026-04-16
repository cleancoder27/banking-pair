using BankingApi.Models;

namespace BankingApi.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _store = new();

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<IEnumerable<Account>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult(_store.Values.AsEnumerable());

    public Task<Account> AddAsync(Account account, CancellationToken ct = default)
    {
        _store[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<Account> UpdateAsync(Account account, CancellationToken ct = default)
    {
        _store[account.Id] = account;
        return Task.FromResult(account);
    }
}
