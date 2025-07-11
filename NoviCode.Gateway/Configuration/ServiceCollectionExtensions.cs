using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoviCode.Core.Abstractions;
using NoviCode.Gateway.Resilience;
using NoviCode.Gateway.Services;

namespace NoviCode.Gateway.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ECB currency gateway with a typed HttpClient.
    /// </summary>
    public static IServiceCollection AddEcbCurrencyGateway(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EcbGatewayOptions>(configuration.GetSection("Gateway:ECB"));

        services.AddHttpClient<ICurrencyGateway, EcbCurrencyGateway>(client =>
        {
            var opts = services.BuildServiceProvider()
                .GetRequiredService<IOptions<EcbGatewayOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/xml");
        })
        .AddPolicyHandler((sp, _) =>
            ResiliencePolicies.GetRetryPolicy(
                sp.GetRequiredService<ILogger<EcbCurrencyGateway>>()))
        .AddPolicyHandler((sp, _) =>
            ResiliencePolicies.GetCircuitBreakerPolicy(
                sp.GetRequiredService<ILogger<EcbCurrencyGateway>>()))
        .AddPolicyHandler(ResiliencePolicies.GetTimeoutPolicy());;

        return services;
    }
}