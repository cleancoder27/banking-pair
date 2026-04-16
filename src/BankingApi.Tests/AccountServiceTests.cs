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
        var account = await svc.OpenAccountAsync("Alice", "ACC-001", 500m);
        Assert.Equal(500m, account.Balance);
    }
}
