using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NoviCode.Api.Attributes;
using NoviCode.Api.Extensions;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;

namespace NoviCode.Api.Controllers;

[ApiController]
[Route("api/wallets")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)] 
public class WalletsController : ControllerBase
{
    private readonly IWalletFacade _walletFacade;

    public WalletsController(IWalletFacade walletFacade)
    {
        _walletFacade = walletFacade ?? throw new ArgumentNullException(nameof(walletFacade));
    }
    
    /// <summary>
    /// Get the wallet by its ID. If a currency is specified, the balance will be converted to that currency.
    /// </summary>
    /// <param name="walletId">The id of the wallet </param>
    /// <param name="currency">The target currency to convert the wallet into</param>
    /// <returns>The requested wallet</returns>
    [HttpGet("{walletId:long}", Name = nameof(GetWalletAsync))]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> GetWalletAsync([FromRoute, Range(1, long.MaxValue)] long walletId, [FromQuery] string? currency)
    {
        var result = await _walletFacade.GetWalletAsync(walletId, currency);
        return result.ToActionResult();
    }
    
    /// <summary>
    /// Creates a new wallet with the specified starting balance and currency.
    /// </summary>
    /// <param name="request">The request body encapsulating startingBalance(decimal) and currency(3 letter supported currency)</param>
    /// <returns>Created Result with wallet</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WalletDto>> CreateWalletAsync([FromBody] CreateWalletRequest request)
    {
        var result = await _walletFacade.CreateWalletAsync(request);
        return result.IsSuccess 
            ? CreatedAtRoute(nameof(GetWalletAsync), new { walletId = result.Value.Id }, result.Value) 
            : result.ToActionResult();
    }
    
    /// <summary>
    /// Adjusts the balance of a wallet by a specified amount and strategy.
    /// </summary>
    /// <remarks>
    /// <para><b>AddFunds</b>: Adds the specified amount to the wallet's balance.</para>  
    /// <para><b>SubtractFunds</b>: Subtracts the specified amount from the wallet's balance.</para>  
    /// <para><b>ForceSubtractFunds</b>: Subtracts the specified amount, even into negative.</para>  
    /// </remarks>
    [HttpPost("{walletId:long}/adjustbalance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> AdjustBalanceAsync(
        [FromRoute, Range(1, long.MaxValue)] long walletId,
        [FromQuery, PositiveDecimal(ErrorMessage = "Amount must be a positive decimal.")] decimal amount,
        [FromQuery, Required(AllowEmptyStrings = false), BindRequired, Length(1, 3, ErrorMessage = "Currency is required.")]  string currency,
        [FromQuery, EnumDataType(typeof(Strategy))] Strategy strategy)
    {
        var request = new AdjustBalanceRequest(currency, walletId, strategy, amount);
        var response = await _walletFacade.AdjustBalanceAsync(request);
        return response.ToActionResult();
    }
}