using PurchaseManagement.Api.DTOs;

namespace PurchaseManagement.Api.Services;

public interface IPurchaseBillService
{
    Task<PurchaseBillResponseDto> CreatePurchaseBillAsync(PurchaseBillRequestDto request);
    Task<PurchaseBillListResponseDto> GetAllPurchaseBillsAsync();
    Task<ItemSummaryDto> GetSummaryAsync();
    IReadOnlyList<string> GetAllowedItems();
    decimal CalculateTotalCost(decimal standardCost, int quantity, decimal discount);
    decimal CalculateTotalSelling(decimal standardPrice, int quantity);
}
