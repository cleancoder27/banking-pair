using BankingApi.Models;
using BankingApi.Repositories;
using BankingApi.Services;
using Xunit;

namespace BankingApi.Tests;

public class AccountServiceTests
{
    private static AccountService BuildService() =>
        new AccountService(new InMemoryAccountRepository());

    [Fact]
    public async Task OpenAccount_WithValidDeposit_ReturnsAccount()
    {
        var svc = BuildService();
        var account = await svc.OpenAccountAsync("Alice", "ACC-001", 500m, "Checking");
        Assert.Equal(500m, account.Balance);
    }

    [Fact]
    public async Task Withdraw_SufficientFunds_ReturnsTrue()
    {
        var svc = BuildService();
        var account = await svc.OpenAccountAsync("Bob", "ACC-002", 500m, "Checking");
        var result = await svc.WithdrawAsync(account.Id, 100m);
        Assert.True(result);
    }

    [Fact]
    public async Task SavingsAccount_Balance_BehavesLikeBaseAccount()
    {
        Account account = new SavingsAccount();
        account.Balance = -50m;
        Assert.Equal(-50m, account.Balance); // will NOT equal -50 on SavingsAccount
    }
}
