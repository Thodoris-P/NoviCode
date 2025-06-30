using System.Xml.Serialization;

namespace NoviCode.Gateway.Models;
[XmlRoot("Envelope", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
public class Envelope
{
    [XmlElement("subject", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public string Subject { get; set; }

    [XmlElement("Sender", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public Sender Sender { get; set; }

    // this maps the outer <Cube> wrapper
    [XmlElement("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public CubeWrapper Cube { get; set; }
}

public class Sender
{
    [XmlElement("name", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public string Name { get; set; }
}

public class CubeWrapper
{
    // maps each <Cube time="…">
    [XmlElement("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public DailyRates[] DailyRates { get; set; }
}

public class DailyRates
{
    [XmlAttribute("time")]
    public DateTime Time { get; set; }

    // maps each <Cube currency="…" rate="…"/>
    [XmlElement("Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public RateEntry[] Rates { get; set; }
}

public class RateEntry
{
    [XmlAttribute("currency")]
    public string Currency { get; set; }

    [XmlAttribute("rate")]
    public decimal Rate { get; set; }
}