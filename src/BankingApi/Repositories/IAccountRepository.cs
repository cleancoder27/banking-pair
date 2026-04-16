using BankingApi.Models;

namespace BankingApi.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetAllAsync(CancellationToken ct = default);
    Task<Account> AddAsync(Account account, CancellationToken ct = default);
    Task<Account> UpdateAsync(Account account, CancellationToken ct = default);
}
