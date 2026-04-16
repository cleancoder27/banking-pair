namespace BankingApi.Exceptions;

public class InsufficientFundsException : Exception
{
    public Guid AccountId { get; }
    public decimal Available { get; }
    public decimal Requested { get; }

    public InsufficientFundsException(Guid accountId, decimal available, decimal requested)
        : base($"Account {accountId} has insufficient funds. Available: {available}, Requested: {requested}.")
    {
        AccountId = accountId;
        Available = available;
        Requested = requested;
    }
}
