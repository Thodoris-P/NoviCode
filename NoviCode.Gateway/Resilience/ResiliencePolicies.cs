using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace NoviCode.Gateway.Resilience;

public class ResiliencePolicies
{
    // Retry 3 times with exponential backoff + jitter
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>() // also retry on timeout
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
                                                       + TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                    logger.LogWarning(
                        "Delaying for {delay}ms, then making retry {retry}.",
                        timespan.TotalMilliseconds,
                        retryAttempt)
            );

    // Break the circuit after 5 consecutive faults, keep it broken for 30s
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                    logger.LogWarning("Circuit broken! Breaking for {delay}s.", breakDelay.TotalSeconds),
                onReset: () =>
                    logger.LogInformation("Circuit reset; calls are allowed again."),
                onHalfOpen: () =>
                    logger.LogInformation("Circuit in test mode: next call is a trial.")
            );

    // Timeout any request taking longer than 10s
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
}