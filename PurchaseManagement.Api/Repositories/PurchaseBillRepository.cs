using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Models;

namespace PurchaseManagement.Api.Repositories;

public class PurchaseBillRepository : IPurchaseBillRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseBillRepository> _logger;

    public PurchaseBillRepository(ApplicationDbContext context, ILogger<PurchaseBillRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseBill> AddAsync(PurchaseBill purchaseBill)
    {
        await _context.PurchaseBills.AddAsync(purchaseBill);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Saved purchase bill item with ID: {Id}", purchaseBill.Id);
        return purchaseBill;
    }

    public async Task<List<PurchaseBill>> GetAllAsync()
    {
        return await _context.PurchaseBills
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<ItemSummaryDto> GetSummaryAsync()
    {
        var totalItems = await _context.PurchaseBills.CountAsync();
        var totalQuantity = await _context.PurchaseBills.SumAsync(b => (int?)b.Quantity) ?? 0;

        return new ItemSummaryDto
        {
            TotalItems = totalItems,
            TotalQuantity = totalQuantity
        };
    }
}
