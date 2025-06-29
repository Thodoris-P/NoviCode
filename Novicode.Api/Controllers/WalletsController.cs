using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;

namespace Novicode.Api.Controllers;

[ApiController]
[Route("api/wallets")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)] 
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(IWalletService walletService, ILogger<WalletsController> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpGet("{walletId:long}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    //TODO: check validation of walletId
    public async Task<IActionResult> GetWalletAsync([Required] long walletId)
    {
        try
        {
            var response = await _walletService.GetWalletAsync(walletId);
            // TODO: Either make response nullable, or return result from service and match it
            if (response is null)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the wallet with ID {WalletId}.", walletId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateWalletAsync([FromBody] CreateWalletRequest request)
    {
        try
        {
            var createdWallet = await _walletService.CreateWalletAsync(request);
            // return CreatedAtAction(nameof(GetWalletAsync), new { walletId = createdWallet.Id }, createdWallet);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the wallet.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
    
    [HttpPost("{walletId:long}/adjustbalance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //TODO: check validation of request params
    public async Task<IActionResult> AdjustBalanceAsync(
        [Required] long walletId,
        [FromQuery] [Required] decimal amount,
        [FromQuery] [Required] string currency,
        [FromQuery] [Required] Strategy strategy)
    {
        try
        {
            var request = new AdjustBalanceRequest(currency, walletId, strategy, amount);
            var response = await _walletService.AdjustBalanceAsync(request);
            //TODO: Use proper response type for AdjustBalanceAsync and handle errors accordingly
            if (response is null)
            {
                return NotFound($"Wallet with ID {walletId} not found.");
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adjusting the balance for wallet with ID {WalletId}.", walletId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}