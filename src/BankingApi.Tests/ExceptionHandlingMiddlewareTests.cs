using BankingApi.Exceptions;
using BankingApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace BankingApi.Tests;

public class ExceptionHandlingMiddlewareTests
{
    private static ExceptionHandlingMiddleware BuildMiddleware(Exception exToThrow)
    {
        return new ExceptionHandlingMiddleware(
            _ => throw exToThrow,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
    }

    private static DefaultHttpContext BuildContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task AccountNotFoundException_Returns404()
    {
        var middleware = BuildMiddleware(new AccountNotFoundException(Guid.NewGuid()));
        var context = BuildContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InsufficientFundsException_Returns422()
    {
        var middleware = BuildMiddleware(
            new InsufficientFundsException(Guid.NewGuid(), 50m, 100m));
        var context = BuildContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
    }

    [Fact]
    public async Task UnhandledException_Returns500()
    {
        var middleware = BuildMiddleware(new InvalidOperationException("boom"));
        var context = BuildContext();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task Response_ContainsProblemDetailsJson()
    {
        var middleware = BuildMiddleware(new AccountNotFoundException(Guid.NewGuid()));
        var context = BuildContext();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("application/problem+json", context.Response.ContentType);
    }
}
