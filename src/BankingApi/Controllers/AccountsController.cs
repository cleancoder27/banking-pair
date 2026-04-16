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
            request.OwnerName, request.AccountNumber, request.InitialDeposit, ct);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }
}

public record OpenAccountRequest(
    string OwnerName,
    string AccountNumber,
    decimal InitialDeposit);
