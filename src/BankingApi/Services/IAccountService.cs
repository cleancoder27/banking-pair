using BankingApi.Models;

namespace BankingApi.Services;

public interface IAccountService
{
    Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default);
    Task<Account> OpenAccountAsync(string ownerName, string accountNumber,
        decimal initialDeposit, CancellationToken ct = default);
}
