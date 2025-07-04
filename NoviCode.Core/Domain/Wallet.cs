using System.ComponentModel.DataAnnotations;

namespace NoviCode.Core.Domain;

public class Wallet
{
    public long Id { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}