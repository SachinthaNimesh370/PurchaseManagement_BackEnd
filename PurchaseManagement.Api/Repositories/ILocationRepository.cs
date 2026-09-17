using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Models;

namespace PurchaseManagement.Api.Repositories;

public interface ILocationRepository
{
    Task<List<Location>> GetAllLocationsAsync();
    Task SaveLocationsAsync(IEnumerable<LocationDto> locations);
}
