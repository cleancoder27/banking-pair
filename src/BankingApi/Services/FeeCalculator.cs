namespace BankingApi.Services;

public class FeeCalculator
{
    public decimal Calculate(string accountType, decimal amount)
    {
        if (accountType == "Checking")
            return 0.50m;
        else if (accountType == "Savings")
            return 0.25m;
        else if (accountType == "Premium")
            return 0m;
        else
            return 1.00m;
    }
}
