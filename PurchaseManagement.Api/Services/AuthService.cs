using System.Net.Http.Json;
using System.Text.Json;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Repositories;

namespace PurchaseManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocationRepository _locationRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        HttpClient httpClient,
        ILocationRepository locationRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _locationRepository = locationRepository;
        _jwtService = jwtService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var endpoint = _configuration["ExternalApi:LoginUrl"] 
            ?? Environment.GetEnvironmentVariable("EXTERNAL_LOGIN_API_URL") 
            ?? "https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke";

        var payload = new ExternalLoginRequestDto
        {
            API_Action = "GetLoginData",
            Device_Id = "D001",
            Sync_Time = "",
            Company_Code = request.Email,
            API_Body = new ExternalLoginRequestBody
            {
                Username = request.Email,
                Pw = request.Password
            }
        };

        HttpResponseMessage httpResponse;
        try
        {
            _logger.LogInformation("Calling external Enhanzer login API for user {Email}", request.Email);
            var serializedPayload = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(serializedPayload, System.Text.Encoding.UTF8, "application/json");
            httpResponse = await _httpClient.PostAsync(endpoint, httpContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to external Enhanzer API at {Url}", endpoint);
            return new LoginResponseDto
            {
                Success = false,
                Message = "Unable to authenticate. Please try again."
            };
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("External API returned non-success status code {StatusCode}", httpResponse.StatusCode);
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        string rawResponse;
        try
        {
            rawResponse = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("External API response received: {Response}", rawResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading response content from external API");
            return new LoginResponseDto
            {
                Success = false,
                Message = "Unable to authenticate. Please try again."
            };
        }

        // Parse external response and extract User_Locations
        var userLocations = new List<LocationDto>();
        bool isLoginSuccessful = false;

        try
        {
            using var jsonDoc = JsonDocument.Parse(rawResponse);
            var root = jsonDoc.RootElement;

            // Check status code in external response
            if (root.TryGetProperty("Status_Code", out var statusProp) && statusProp.GetInt32() == 200)
            {
                isLoginSuccessful = true;
            }
            else if (root.TryGetProperty("Success", out var successProp) && successProp.GetBoolean())
            {
                isLoginSuccessful = true;
            }

            // Extract User_Locations: Handles Response_Body as Array, Object, or Root
            void ExtractLocationsFromArray(JsonElement locArray)
            {
                if (locArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in locArray.EnumerateArray())
                    {
                        var locCode = item.TryGetProperty("Location_Code", out var c) ? c.GetString() ?? "" : "";
                        var locName = item.TryGetProperty("Location_Name", out var n) ? n.GetString() ?? "" : "";

                        if (!string.IsNullOrWhiteSpace(locCode) || !string.IsNullOrWhiteSpace(locName))
                        {
                            userLocations.Add(new LocationDto
                            {
                                Location_Code = locCode,
                                Location_Name = locName
                            });
                        }
                    }
                }
            }

            if (root.TryGetProperty("Response_Body", out var responseBody))
            {
                if (responseBody.ValueKind == JsonValueKind.Array)
                {
                    foreach (var userObj in responseBody.EnumerateArray())
                    {
                        if (userObj.TryGetProperty("User_Locations", out var locs))
                        {
                            ExtractLocationsFromArray(locs);
                        }
                    }
                }
                else if (responseBody.ValueKind == JsonValueKind.Object)
                {
                    if (responseBody.TryGetProperty("Doc_Msg", out var docMsgProp))
                    {
                        var docMsg = docMsgProp.GetString();
                        if (!string.IsNullOrWhiteSpace(docMsg) && docMsg.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                        {
                            isLoginSuccessful = false;
                        }
                    }

                    if (responseBody.TryGetProperty("User_Locations", out var locs))
                    {
                        ExtractLocationsFromArray(locs);
                    }
                }
            }

            // Also check root level as fallback
            if (!userLocations.Any() && root.TryGetProperty("User_Locations", out var rootLocs))
            {
                ExtractLocationsFromArray(rootLocs);
            }

            // Login requires successful location retrieval
            if (userLocations.Any())
            {
                isLoginSuccessful = true;
            }
            else
            {
                isLoginSuccessful = false;
            }

            // Check for explicit error message from external API
            if (root.TryGetProperty("Message", out var msgProp))
            {
                var msg = msgProp.GetString();
                if (!string.IsNullOrWhiteSpace(msg) && (msg.Contains("invalid", StringComparison.OrdinalIgnoreCase) || msg.Contains("fail", StringComparison.OrdinalIgnoreCase)))
                {
                    isLoginSuccessful = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from external API response");
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        if (!isLoginSuccessful)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        // Persist User_Locations to SQL Server via EF Core repository
        try
        {
            if (userLocations.Any())
            {
                await _locationRepository.SaveLocationsAsync(userLocations);
                _logger.LogInformation("Persisted {Count} locations into SQL Server", userLocations.Count);
            }
            else
            {
                _logger.LogWarning("No locations extracted from external login response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist user locations into SQL Server database");
        }

        // Issue JWT token
        var token = _jwtService.GenerateToken(request.Email);

        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            Email = request.Email,
            UserLocations = userLocations
        };
    }
}
