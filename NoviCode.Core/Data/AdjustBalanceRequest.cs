namespace NoviCode.Core.Data;

public record AdjustBalanceRequest(string Currency, long WalletId, Strategy Strategy, decimal Amount);