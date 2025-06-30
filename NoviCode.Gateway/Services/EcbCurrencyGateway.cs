using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Gateway.Models;
using NoviCode.Gateway.Utils;

namespace NoviCode.Gateway.Services;

public class EcbCurrencyGateway : ICurrencyGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EcbCurrencyGateway> _logger;
    private readonly string contextPath = "stats/eurofxref/eurofxref-daily.xml";

    public EcbCurrencyGateway(HttpClient httpClient, ILogger<EcbCurrencyGateway> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<ExchangeRate>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(contextPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        var serializer = new XmlSerializer(typeof(Envelope));
        var result = (Envelope)serializer.Deserialize(stream);

        return EcbRatesToExchangeRatesMapper.Map(result);
    }
}