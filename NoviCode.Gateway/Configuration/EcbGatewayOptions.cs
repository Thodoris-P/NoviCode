namespace NoviCode.Gateway.Configuration;

public class EcbGatewayOptions
{
    /// <summary>
    /// Base URL for the ECB API (e.g. "https://www.ecb.europa.eu/").
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.ecb.europa.eu/";
        
    /// <summary>
    /// Relative path to fetch the daily rates XML.
    /// </summary>
    public string RelativeUrl { get; set; } = "stats/eurofxref/eurofxref-daily.xml";
        
    /// <summary>
    /// Client timeout in seconds when fetching rates.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}