using BankingApi.Middleware;
using BankingApi.Repositories;
using BankingApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1st — outermost, catches everything
app.UseMiddleware<RequestLoggingMiddleware>();     // 2nd — logs every request with correlation ID
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
