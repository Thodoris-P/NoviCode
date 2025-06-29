using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Novicode.Api.Configuration;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Services;
using NoviCode.Data.Data;

var builder = WebApplication.CreateBuilder(args);

#region configuration
// Bind DatabaseSettings section to IOptions<DatabaseSettings>
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings")
);
#endregion

#region Core Services
builder.Services.AddDbContext<NoviCodeContext>((serviceProvider, options) =>
{
    var dbSettings = serviceProvider
        .GetRequiredService<IOptions<DatabaseSettings>>()
        .Value;
    options.UseSqlServer(dbSettings.ConnectionString);
});

builder.Services.AddControllers();

// Register Rate Limiter Policy
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("DefaultPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            })
    );
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
#endregion

#region Application Services
builder.Services.AddScoped<IWalletService,WalletService>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ICurrencyConverter, CurrencyConverter>();
builder.Services.AddTransient<IAdjustFundsStrategy, AddFundsStrategy>();
builder.Services.AddTransient<IAdjustFundsStrategy, SubtractFundsStrategy>();
builder.Services.AddTransient<IAdjustFundsStrategy, ForceSubtractFundsStrategy>();
builder.Services.AddSingleton<IAdjustFundsFactory, AdjustFundsFactory>();

#endregion

var app = builder.Build();

// Enable rate limiting middleware
app.UseRateLimiter();

// Map controllers and apply rate limiting policy
app.MapControllers()
    .RequireRateLimiting("DefaultPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();
