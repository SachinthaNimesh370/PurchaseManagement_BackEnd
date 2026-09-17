namespace PurchaseManagement.Api.DTOs;

public class PurchaseBillListResponseDto
{
    public List<PurchaseBillResponseDto> Items { get; set; } = new();
    public ItemSummaryDto Summary { get; set; } = new();
}
