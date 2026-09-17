using System.Text.Json.Serialization;

namespace PurchaseManagement.Api.DTOs;

public class ExternalLoginResponseDto
{
    [JsonPropertyName("Status_Code")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("Success")]
    public bool? Success { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("User_Locations")]
    public List<LocationDto>? UserLocations { get; set; }

    [JsonPropertyName("User_Details")]
    public object? UserDetails { get; set; }
}
