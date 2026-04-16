using BankingApi.Models;
using BankingApi.Repositories;

namespace BankingApi.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly SmtpNotificationSender _notificationSender;
    private readonly FeeCalculator _feeCalculator;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
        _notificationSender = new SmtpNotificationSender();
        _feeCalculator = new FeeCalculator();
    }

    public Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, ct);

    public Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken ct = default) =>
        _repository.GetAllAsync(ct);

    public async Task<Account> OpenAccountAsync(string ownerName, string accountNumber,
        decimal initialDeposit, string accountType, CancellationToken ct = default)
    {
        if (initialDeposit < 0)
            throw new ArgumentException("Initial deposit cannot be negative.",
                nameof(initialDeposit));

        var account = new Account
        {
            OwnerName = ownerName,
            AccountNumber = accountNumber,
            Balance = initialDeposit,
            AccountType = accountType
        };
        return await _repository.AddAsync(account, ct);
    }

    public async Task<bool> WithdrawAsync(Guid accountId, decimal amount,
        CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(accountId, ct);
        if (account == null) return false;

        decimal fee = _feeCalculator.Calculate(account.AccountType, amount);
        decimal total = amount + fee;

        if (account.AccountType == "Checking")
        {
            if (account.Balance < total) return false;
            account.Balance -= total;
        }
        else if (account.AccountType == "Savings")
        {
            if (account.Balance < total) return false;
            if (account.Balance - total < 100)
                return false;
            account.Balance -= total;
        }
        else if (account.AccountType == "Premium")
        {
            if (account.Balance < amount) return false;
            account.Balance -= amount;
        }

        await _repository.UpdateAsync(account, ct);
        await _notificationSender.SendAsync(account.OwnerName,
            $"Withdrawal of {amount:C} processed on account {account.AccountNumber}.");
        return true;
    }

    public async Task<bool> DepositAsync(Guid accountId, decimal amount,
        CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(accountId, ct);
        if (account == null) return false;
        account.Balance += amount;
        await _repository.UpdateAsync(account, ct);
        return true;
    }

    public async Task<string> GenerateAccountReportAsync(CancellationToken ct = default)
    {
        var accounts = await _repository.GetAllAsync(ct);
        var lines = accounts.Select(a =>
            $"{a.AccountNumber} | {a.OwnerName} | {a.AccountType} | {a.Balance:C}");
        return string.Join(Environment.NewLine, lines);
    }

    public async Task<bool> SendNotificationAsync(Guid accountId, string message,
        CancellationToken ct = default)
    {
        var account = await _repository.GetByIdAsync(accountId, ct);
        if (account == null) return false;
        await _notificationSender.SendAsync(account.OwnerName, message);
        return true;
    }
}
