using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PurchaseManagement.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string email)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
            ?? "EnhanzerSecretKeyForJwtAuthentication2026_LongSecureKey!";
        
        var issuer = _configuration["Jwt:Issuer"] 
            ?? Environment.GetEnvironmentVariable("JWT_ISSUER") 
            ?? "PurchaseManagementApi";

        var audience = _configuration["Jwt:Audience"] 
            ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
            ?? "PurchaseManagementClient";

        var expiryHoursStr = _configuration["Jwt:ExpiryHours"] 
            ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS") 
            ?? "8";

        if (!double.TryParse(expiryHoursStr, out var expiryHours))
        {
            expiryHours = 8;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, email)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
