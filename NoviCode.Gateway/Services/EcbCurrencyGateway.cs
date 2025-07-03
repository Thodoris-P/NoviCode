using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Gateway.Exceptions;
using NoviCode.Gateway.Models;

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

    public async Task<Envelope> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(contextPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
            var serializer = new XmlSerializer(typeof(Envelope));
            if (serializer.Deserialize(stream) is Envelope result)
                return result;
            
            throw new ArgumentNullException(nameof(result), "Received an empty or unrecognized response when deserializing currency rates.");
        }
        catch (Exception e)
        {
            throw new CurrencyGatewayException("Failed to retrieve or parse currency rates.", e);
        }
    }
}