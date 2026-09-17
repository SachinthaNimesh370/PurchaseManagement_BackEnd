using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Models;

namespace PurchaseManagement.Api.Repositories;

public interface IPurchaseBillRepository
{
    Task<PurchaseBill> AddAsync(PurchaseBill purchaseBill);
    Task<List<PurchaseBill>> GetAllAsync();
    Task<ItemSummaryDto> GetSummaryAsync();
}
