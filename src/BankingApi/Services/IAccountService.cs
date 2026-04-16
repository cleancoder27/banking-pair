using BankingApi.Models;

namespace BankingApi.Services;

public interface IAccountService
{
    Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default);
    Task<Account> OpenAccountAsync(string ownerName, string accountNumber,
        decimal initialDeposit, CancellationToken ct = default);
    Task DebitAsync(Guid accountId, decimal amount, CancellationToken ct = default);
    Task CreditAsync(Guid accountId, decimal amount, CancellationToken ct = default);
    Task<IEnumerable<TransferResult>> ProcessBatchAsync(
        IEnumerable<TransferRequest> requests,
        CancellationToken ct = default);
}

public record TransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);
public record TransferResult(Guid FromAccountId, Guid ToAccountId, bool Success, string? FailureReason);
