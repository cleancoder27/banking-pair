using BankingApi.Exceptions;
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

    [Fact]
    public async Task Debit_SufficientFunds_ReducesBalance()
    {
        var svc = BuildService();
        var account = await svc.OpenAccountAsync("Alice", "ACC-001", 500m);

        await svc.DebitAsync(account.Id, 100m);

        var updated = await svc.GetAccountAsync(account.Id);
        Assert.Equal(400m, updated!.Balance);
    }

    [Fact]
    public async Task Debit_InsufficientFunds_ThrowsInsufficientFundsException()
    {
        var svc = BuildService();
        var account = await svc.OpenAccountAsync("Bob", "ACC-002", 50m);

        await Assert.ThrowsAsync<InsufficientFundsException>(
            () => svc.DebitAsync(account.Id, 100m));
    }

    [Fact]
    public async Task Debit_AccountNotFound_ThrowsAccountNotFoundException()
    {
        var svc = BuildService();

        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => svc.DebitAsync(Guid.NewGuid(), 100m));
    }

    [Fact]
    public async Task ProcessBatchAsync_UsesDictionaryLookup_NotNestedLoop()
    {
        var svc = BuildService();
        var alice = await svc.OpenAccountAsync("Alice", "ACC-001", 500m);
        var bob   = await svc.OpenAccountAsync("Bob",   "ACC-002", 0m);

        var requests = new[]
        {
            new TransferRequest(alice.Id, bob.Id, 200m),
            new TransferRequest(alice.Id, bob.Id, 100m),
        };

        var results = (await svc.ProcessBatchAsync(requests)).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success));

        var updatedAlice = await svc.GetAccountAsync(alice.Id);
        Assert.Equal(200m, updatedAlice!.Balance);
    }

    [Fact]
    public async Task ProcessBatchAsync_PartialFailure_ContinuesAndReportsFailure()
    {
        var svc = BuildService();
        var alice = await svc.OpenAccountAsync("Alice", "ACC-001", 50m);
        var bob   = await svc.OpenAccountAsync("Bob",   "ACC-002", 0m);

        var requests = new[]
        {
            new TransferRequest(alice.Id, bob.Id, 200m),  // should fail — insufficient funds
            new TransferRequest(alice.Id, bob.Id, 30m),   // should succeed
        };

        var results = (await svc.ProcessBatchAsync(requests)).ToList();

        Assert.False(results[0].Success);
        Assert.True(results[1].Success);
    }
}
