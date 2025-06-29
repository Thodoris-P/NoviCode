using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(long id);
    Task AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
}