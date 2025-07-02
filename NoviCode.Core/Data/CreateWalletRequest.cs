using System.ComponentModel.DataAnnotations;

namespace NoviCode.Core.Data;

public class CreateWalletRequest
{
    [Required(ErrorMessage = "Currency is required.")]
    public required string Currency { get; set; }
    
    // Enforce non-negative starting balance
    [Range(0, double.MaxValue, ErrorMessage = "Starting balance must be zero or positive.")]
    public decimal StartingBalance { get; set; }
}