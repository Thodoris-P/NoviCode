namespace NoviCode.Core.Data;

public class CreateWalletRequest
{
    public required string Currency { get; set; }
    public decimal StartingBalance { get; set; }
}