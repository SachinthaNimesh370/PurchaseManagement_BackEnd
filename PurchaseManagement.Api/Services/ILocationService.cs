using PurchaseManagement.Api.DTOs;

namespace PurchaseManagement.Api.Services;

public interface ILocationService
{
    Task<List<LocationDto>> GetLocationsAsync();
}
