using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Repositories;

namespace PurchaseManagement.Api.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<LocationService> _logger;

    public LocationService(ILocationRepository locationRepository, ILogger<LocationService> logger)
    {
        _locationRepository = locationRepository;
        _logger = logger;
    }

    public async Task<List<LocationDto>> GetLocationsAsync()
    {
        try
        {
            var locations = await _locationRepository.GetAllLocationsAsync();
            return locations.Select(l => new LocationDto
            {
                Location_Code = l.LocationCode,
                Location_Name = l.LocationName
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve locations from repository");
            throw;
        }
    }
}
