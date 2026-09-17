using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Models;
using PurchaseManagement.Api.Repositories;

namespace PurchaseManagement.Api.Services;

public class PurchaseBillService : IPurchaseBillService
{
    private static readonly string[] AllowedItemList = new[]
    {
        "Mango",
        "Apple",
        "Banana",
        "Orange",
        "Grapes",
        "Kiwi",
        "Strawberry"
    };

    private static readonly HashSet<string> AllowedItemSet = new(AllowedItemList, StringComparer.OrdinalIgnoreCase);

    private readonly IPurchaseBillRepository _repository;
    private readonly ILogger<PurchaseBillService> _logger;

    public PurchaseBillService(IPurchaseBillRepository repository, ILogger<PurchaseBillService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public IReadOnlyList<string> GetAllowedItems() => AllowedItemList;

    public decimal CalculateTotalCost(decimal standardCost, int quantity, decimal discount)
    {
        if (standardCost < 0) throw new ArgumentException("Standard Cost cannot be negative", nameof(standardCost));
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        if (discount < 0 || discount > 100) throw new ArgumentException("Discount must be between 0 and 100", nameof(discount));

        var gross = standardCost * quantity;
        var discountAmount = gross * (discount / 100m);
        return Math.Round(gross - discountAmount, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateTotalSelling(decimal standardPrice, int quantity)
    {
        if (standardPrice < 0) throw new ArgumentException("Standard Price cannot be negative", nameof(standardPrice));
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        return Math.Round(standardPrice * quantity, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<PurchaseBillResponseDto> CreatePurchaseBillAsync(PurchaseBillRequestDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Validate item against allowed list
        var trimmedItem = request.Item?.Trim() ?? string.Empty;
        var matchedItem = AllowedItemList.FirstOrDefault(i => i.Equals(trimmedItem, StringComparison.OrdinalIgnoreCase));
        if (matchedItem == null)
        {
            throw new ArgumentException(
                $"Invalid item '{request.Item}'. Allowed items are: {string.Join(", ", AllowedItemList)}",
                nameof(request.Item));
        }

        if (string.IsNullOrWhiteSpace(request.Batch))
        {
            throw new ArgumentException("Batch is required", nameof(request.Batch));
        }

        // Calculate Total Cost and Total Selling using exact formulas
        var totalCost = CalculateTotalCost(request.StandardCost, request.Quantity, request.Discount);
        var totalSelling = CalculateTotalSelling(request.StandardPrice, request.Quantity);

        var entity = new PurchaseBill
        {
            Item = matchedItem,
            Batch = request.Batch.Trim(),
            StandardCost = request.StandardCost,
            StandardPrice = request.StandardPrice,
            Quantity = request.Quantity,
            Discount = request.Discount,
            TotalCost = totalCost,
            TotalSelling = totalSelling,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(entity);

        return MapToDto(saved);
    }

    public async Task<PurchaseBillListResponseDto> GetAllPurchaseBillsAsync()
    {
        var entities = await _repository.GetAllAsync();
        var summary = await _repository.GetSummaryAsync();

        return new PurchaseBillListResponseDto
        {
            Items = entities.Select(MapToDto).ToList(),
            Summary = summary
        };
    }

    public async Task<ItemSummaryDto> GetSummaryAsync()
    {
        return await _repository.GetSummaryAsync();
    }

    private static PurchaseBillResponseDto MapToDto(PurchaseBill bill)
    {
        return new PurchaseBillResponseDto
        {
            Id = bill.Id,
            Item = bill.Item,
            Batch = bill.Batch,
            StandardCost = bill.StandardCost,
            StandardPrice = bill.StandardPrice,
            Quantity = bill.Quantity,
            Discount = bill.Discount,
            TotalCost = bill.TotalCost,
            TotalSelling = bill.TotalSelling,
            CreatedAt = bill.CreatedAt
        };
    }
}
