namespace BankingApi.Models;

public class SavingsAccount : Account
{
    private decimal _balance;

    public override decimal Balance
    {
        get => _balance;
        set => _balance = value < 0 ? 0 : value;
    }

    public SavingsAccount()
    {
        AccountType = "Savings";
    }
}
