using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Novicode.Api.Configuration;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Services;
using NoviCode.Data.Data;
using NoviCode.Gateway.Configuration;
using Quartz;
using Serilog;

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

builder.Services.AddStackExchangeRedisCache(options =>
{
    builder.Configuration.GetSection("Redis").Bind(options);
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

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));
#endregion

#region Application Services
builder.Services.AddScoped<IWalletFacade, WalletFacade>();
builder.Services.AddScoped<IWalletService,WalletService>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ICurrencyConverter, CurrencyConverter>();
builder.Services.AddTransient<IAdjustFundsStrategy, AddFundsStrategy>();
builder.Services.AddTransient<IAdjustFundsStrategy, SubtractFundsStrategy>();
builder.Services.AddTransient<IAdjustFundsStrategy, ForceSubtractFundsStrategy>();
builder.Services.AddSingleton<IAdjustFundsFactory, AdjustFundsFactory>();
builder.Services.AddScoped<IExchangeRatesService, ExchangeRatesService>();
builder.Services.AddScoped<ExchangeRatesRepository>();
builder.Services.AddScoped<IExchangeRatesRepository>(sp =>
{
    var repo = sp.GetRequiredService<ExchangeRatesRepository>();
    var cache = sp.GetRequiredService<IDistributedCache>();
    var logger = sp.GetRequiredService<ILogger<CachedExchangeRatesRepository>>();
    return new CachedExchangeRatesRepository(logger, repo, cache);
});
builder.Services.AddEcbCurrencyGateway(builder.Configuration);
#endregion

#region Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("UpdateRatesJob");
    q.AddJob<UpdateRatesJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("UpdateRatesTrigger")
        .WithCronSchedule("0 * * * * ?")
    );
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
#endregion

#region Logger
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add(HeaderNames.Accept);
    logging.RequestHeaders.Add(HeaderNames.ContentType);
    logging.RequestHeaders.Add(HeaderNames.ContentDisposition);
    logging.RequestHeaders.Add(HeaderNames.ContentEncoding);
    logging.RequestHeaders.Add(HeaderNames.ContentLength);
            
    logging.MediaTypeOptions.AddText("application/json");
    logging.MediaTypeOptions.AddText("multipart/form-data");
            
    logging.RequestBodyLogLimit = 1024;
    logging.ResponseBodyLogLimit = 1024;
});


#endregion

var app = builder.Build();

app.UseHttpLogging();
app.UseSerilogRequestLogging();

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
