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
    
    [HttpGet("{walletId:long}", Name = nameof(GetWalletAsync))]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> GetWalletAsync([FromRoute, Range(1, long.MaxValue)] long walletId, [FromQuery] string? currency)
    {
        var result = await _walletFacade.GetWalletAsync(walletId, currency);
        return result.ToActionResult();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WalletDto>> CreateWalletAsync([FromBody] CreateWalletRequest request)
    {
        var result = await _walletFacade.CreateWalletAsync(request);
        return result.IsSuccess 
            ? CreatedAtRoute(nameof(GetWalletAsync), new { walletId = result.Value.Id }, result.Value) 
            : result.ToActionResult();
    }
    
    [HttpPost("{walletId:long}/adjustbalance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> AdjustBalanceAsync(
        [FromRoute, Range(1, long.MaxValue)] long walletId,
        [FromQuery, PositiveDecimal(ErrorMessage = "Amount must be a positive decimal.")] decimal amount,
        [FromQuery, Required(AllowEmptyStrings = false), BindRequired, MinLength(1, ErrorMessage = "Currency is required.")]  string currency,
        [FromQuery, EnumDataType(typeof(Strategy))] Strategy strategy)
    {
        var request = new AdjustBalanceRequest(currency, walletId, strategy, amount);
        var response = await _walletFacade.AdjustBalanceAsync(request);
        return response.ToActionResult();
    }
}