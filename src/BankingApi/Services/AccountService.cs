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
}
