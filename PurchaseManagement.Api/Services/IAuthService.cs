using PurchaseManagement.Api.DTOs;

namespace PurchaseManagement.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
