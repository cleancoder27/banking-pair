using BankingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accounts;

    public AccountsController(IAccountService accounts)
    {
        _accounts = accounts;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _accounts.GetAllAccountsAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var account = await _accounts.GetAccountAsync(id, ct);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> OpenAccount(
        [FromBody] OpenAccountRequest request, CancellationToken ct)
    {
        var account = await _accounts.OpenAccountAsync(
            request.OwnerName, request.AccountNumber,
            request.InitialDeposit, request.AccountType, ct);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPost("{id:guid}/deposit")]
    public async Task<IActionResult> Deposit(Guid id,
        [FromBody] AmountRequest request, CancellationToken ct)
    {
        var success = await _accounts.DepositAsync(id, request.Amount, ct);
        return success ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id,
        [FromBody] AmountRequest request, CancellationToken ct)
    {
        var success = await _accounts.WithdrawAsync(id, request.Amount, ct);
        return success ? Ok() : BadRequest("Withdrawal failed.");
    }

    [HttpGet("report")]
    public async Task<IActionResult> Report(CancellationToken ct) =>
        Ok(await _accounts.GenerateAccountReportAsync(ct));
}

public record OpenAccountRequest(
    string OwnerName,
    string AccountNumber,
    decimal InitialDeposit,
    string AccountType = "Checking");

public record AmountRequest(decimal Amount);
