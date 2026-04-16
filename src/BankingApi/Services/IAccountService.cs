using BankingApi.Models;

namespace BankingApi.Services;

public interface IAccountService
{
    Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default);
    Task<Account> OpenAccountAsync(string ownerName, string accountNumber,
        decimal initialDeposit, string accountType, CancellationToken ct = default);
    Task<bool> DepositAsync(Guid accountId, decimal amount, CancellationToken ct = default);
    Task<bool> WithdrawAsync(Guid accountId, decimal amount, CancellationToken ct = default);
    Task<string> GenerateAccountReportAsync(CancellationToken ct = default);
    Task<bool> SendNotificationAsync(Guid accountId, string message, CancellationToken ct = default);
}
