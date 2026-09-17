using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Services;

namespace PurchaseManagement.Api.Controllers;

[ApiController]
[Route("api/purchase-bills")]
[Authorize]
public class PurchaseBillController : ControllerBase
{
    private readonly IPurchaseBillService _purchaseBillService;
    private readonly ILogger<PurchaseBillController> _logger;

    public PurchaseBillController(IPurchaseBillService purchaseBillService, ILogger<PurchaseBillController> logger)
    {
        _purchaseBillService = purchaseBillService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all purchase bill items along with the overall item summary (Total Items, Total Quantity).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PurchaseBillListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _purchaseBillService.GetAllPurchaseBillsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving purchase bills");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to load purchase bills." });
        }
    }

    /// <summary>
    /// Gets only the item summary (Total Items count and Total Quantity sum).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ItemSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var summary = await _purchaseBillService.GetSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving purchase bill summary");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to load item summary." });
        }
    }

    /// <summary>
    /// Gets the list of predefined allowed fruit items for autocomplete.
    /// </summary>
    [HttpGet("items")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetAllowedItems()
    {
        var items = _purchaseBillService.GetAllowedItems();
        return Ok(items);
    }

    /// <summary>
    /// Adds a new purchase bill item with server-side validation and calculation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseBillResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] PurchaseBillRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _purchaseBillService.CreatePurchaseBillAsync(request);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error when creating purchase bill item");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating purchase bill item");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to create purchase bill item." });
        }
    }
}
