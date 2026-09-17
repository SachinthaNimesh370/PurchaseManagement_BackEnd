using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Models;

namespace PurchaseManagement.Api.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(ApplicationDbContext context, ILogger<LocationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Location>> GetAllLocationsAsync()
    {
        return await _context.Locations
            .AsNoTracking()
            .OrderBy(l => l.LocationName)
            .ToListAsync();
    }

    public async Task SaveLocationsAsync(IEnumerable<LocationDto> locations)
    {
        if (locations == null || !locations.Any())
        {
            return;
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Clear existing locations to refresh with latest user locations
            var existing = await _context.Locations.ToListAsync();
            if (existing.Any())
            {
                _context.Locations.RemoveRange(existing);
                await _context.SaveChangesAsync();
            }

            // Insert fresh locations received from login API
            var newLocations = locations.Select(dto => new Location
            {
                LocationCode = dto.Location_Code ?? string.Empty,
                LocationName = dto.Location_Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Locations.AddRangeAsync(newLocations);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("Successfully saved {Count} locations to Location_Details", newLocations.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while saving locations to database");
            throw;
        }
    }
}
