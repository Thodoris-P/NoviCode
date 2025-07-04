

using Microsoft.EntityFrameworkCore;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Core.Exceptions;
using NoviCode.Data.Data;

namespace NoviCode.Core.Services;

public class WalletRepository : IWalletRepository
{
    private readonly NoviCodeContext _context;

    public WalletRepository(NoviCodeContext context)
    {
        _context = context;
    }
    
    public async Task<Wallet?> GetByIdAsync(long id)
    {
        return await _context.Wallets.FindAsync(id);
    }
    
    public async Task AddAsync(Wallet wallet)
    {
        await _context.Wallets.AddAsync(wallet);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException($"Concurrency conflict when updating wallet {wallet.Id}", ex);
        }
    }
}