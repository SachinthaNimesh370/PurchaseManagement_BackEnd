namespace PurchaseManagement.Api.Services;

public interface IJwtService
{
    string GenerateToken(string email);
}
