using Newtonsoft.Json;

namespace PlatformWellSync.DTOs;

public class LoginRequest
{
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    [JsonProperty("token")]
    public string? Token { get; set; }

    [JsonProperty("bearer")]
    public string? Bearer { get; set; }

    [JsonProperty("accessToken")]
    public string? AccessToken { get; set; }

    public string? GetToken() => Token ?? Bearer ?? AccessToken;
}

// Each record from the API is a PLATFORM with nested WELLS inside
public class PlatformApiResponse
{
    [JsonProperty("id")]
    public int? Id { get; set; }

    [JsonProperty("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonProperty("latitude")]
    public double? Latitude { get; set; }

    [JsonProperty("longitude")]
    public double? Longitude { get; set; }

    [JsonProperty("lastUpdate")]
    public DateTime? LastUpdate { get; set; }

    // Nested wells array
    [JsonProperty("well")]
    public List<WellApiResponse>? Wells { get; set; }

    // Absorb any extra/unknown fields safely
    [JsonExtensionData]
    public IDictionary<string, Newtonsoft.Json.Linq.JToken>? ExtraFields { get; set; }
}

public class WellApiResponse
{
    [JsonProperty("id")]
    public int? Id { get; set; }

    [JsonProperty("platformId")]
    public int? PlatformId { get; set; }

    [JsonProperty("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonProperty("latitude")]
    public double? Latitude { get; set; }

    [JsonProperty("longitude")]
    public double? Longitude { get; set; }

    [JsonProperty("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonProperty("lastUpdate")]
    public DateTime? LastUpdate { get; set; }

    // Also try updatedAt in case some records use it
    [JsonProperty("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    public DateTime? GetUpdatedAt() => LastUpdate ?? UpdatedAt;

    // Absorb any extra/unknown fields safely
    [JsonExtensionData]
    public IDictionary<string, Newtonsoft.Json.Linq.JToken>? ExtraFields { get; set; }
}