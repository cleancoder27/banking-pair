using BankingApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BankingApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            AccountNotFoundException      => (StatusCodes.Status404NotFound,               "Account not found"),
            InsufficientFundsException    => (StatusCodes.Status422UnprocessableEntity,    "Insufficient funds"),
            ArgumentException             => (StatusCodes.Status400BadRequest,             "Invalid argument"),
            _                             => (StatusCodes.Status500InternalServerError,    "An unexpected error occurred")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            Detail = ex.Message
        };

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }
}
