using BankingApi.Exceptions;
using BankingApi.Models;
using BankingApi.Repositories;

namespace BankingApi.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, ct);

    public Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default) =>
        _repository.GetAllAsync(ct);

    public async Task<Account> OpenAccountAsync(string ownerName, string accountNumber,
        decimal initialDeposit, CancellationToken ct = default)
    {
        if (initialDeposit < 0)
            throw new ArgumentException("Initial deposit cannot be negative.",
                nameof(initialDeposit));

        var account = new Account
        {
            OwnerName = ownerName,
            AccountNumber = accountNumber,
            Balance = initialDeposit
        };

        return await _repository.AddAsync(account, ct);
    }

    public async Task DebitAsync(Guid accountId, decimal amount, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(accountId, ct)
            ?? throw new AccountNotFoundException(accountId);

        if (account.Balance < amount)
            throw new InsufficientFundsException(accountId, account.Balance, amount);

        account.Balance -= amount;
        await _repository.UpdateAsync(account, ct);
    }

    public async Task CreditAsync(Guid accountId, decimal amount, CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(accountId, ct)
            ?? throw new AccountNotFoundException(accountId);

        account.Balance += amount;
        await _repository.UpdateAsync(account, ct);
    }

    public async Task<IEnumerable<TransferResult>> ProcessBatchAsync(
        IEnumerable<TransferRequest> requests,
        CancellationToken ct = default)
    {
        var allAccounts = await _repository.GetAllAsync(ct);
        var lookup = allAccounts.ToDictionary(a => a.Id);
        var results = new List<TransferResult>();

        foreach (var req in requests)
        {
            ct.ThrowIfCancellationRequested();

            if (!lookup.TryGetValue(req.FromAccountId, out var from))
            {
                results.Add(new TransferResult(req.FromAccountId, req.ToAccountId,
                    false, $"Source account {req.FromAccountId} not found."));
                continue;
            }

            if (!lookup.TryGetValue(req.ToAccountId, out var to))
            {
                results.Add(new TransferResult(req.FromAccountId, req.ToAccountId,
                    false, $"Destination account {req.ToAccountId} not found."));
                continue;
            }

            if (from.Balance < req.Amount)
            {
                results.Add(new TransferResult(req.FromAccountId, req.ToAccountId,
                    false, $"Insufficient funds. Available: {from.Balance}, Requested: {req.Amount}."));
                continue;
            }

            from.Balance -= req.Amount;
            to.Balance += req.Amount;
            await _repository.UpdateAsync(from, ct);
            await _repository.UpdateAsync(to, ct);

            results.Add(new TransferResult(req.FromAccountId, req.ToAccountId, true, null));
        }

        return results;
    }
}
