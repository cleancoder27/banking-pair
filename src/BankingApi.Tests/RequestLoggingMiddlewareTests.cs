using BankingApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BankingApi.Tests;

public class RequestLoggingMiddlewareTests
{
    private static DefaultHttpContext BuildContext(string method = "GET", string path = "/api/accounts")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var nextCalled = false;
        var middleware = new RequestLoggingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            NullLogger<RequestLoggingMiddleware>.Instance);

        await middleware.InvokeAsync(BuildContext());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestAndResponse()
    {
        var logMessages = new List<string>();
        var logger = new CollectingLogger<RequestLoggingMiddleware>(logMessages);

        var middleware = new RequestLoggingMiddleware(
            _ => Task.CompletedTask, logger);

        await middleware.InvokeAsync(BuildContext("POST", "/api/accounts"));

        Assert.Equal(2, logMessages.Count);
        Assert.Contains("--> POST /api/accounts", logMessages[0]);
        Assert.Contains("<-- POST /api/accounts", logMessages[1]);
    }

    [Fact]
    public async Task InvokeAsync_LogsResponseEvenWhenNextThrows()
    {
        var logMessages = new List<string>();
        var logger = new CollectingLogger<RequestLoggingMiddleware>(logMessages);

        var middleware = new RequestLoggingMiddleware(
            _ => throw new InvalidOperationException("boom"), logger);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(BuildContext()));

        // response line must still be logged even when an exception is thrown
        Assert.Equal(2, logMessages.Count);
    }
}

// Minimal ILogger implementation for capturing log output in tests
public class CollectingLogger<T> : ILogger<T>
{
    private readonly List<string> _messages;
    public CollectingLogger(List<string> messages) => _messages = messages;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
        => _messages.Add(formatter(state, exception));
}
