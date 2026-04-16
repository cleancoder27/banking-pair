namespace BankingApi.Services;

public class SmtpNotificationSender
{
    public Task SendAsync(string recipientName, string message)
    {
        Console.WriteLine($"[SMTP] To: {recipientName} | {message}");
        return Task.CompletedTask;
    }
}
