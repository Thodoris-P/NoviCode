namespace NoviCode.Gateway.Configuration;

public class EcbGatewayOptions
{
    public string BaseUrl { get; set; }
    public string RelativeUrl { get; set; }
    public int TimeoutSeconds { get; set; }
}