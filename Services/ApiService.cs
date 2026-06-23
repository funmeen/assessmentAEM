using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlatformWellSync.DTOs;

namespace PlatformWellSync.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private string? _bearerToken;

    private const string BaseUrl = "http://test-demo.aemenersol.com";
    private const string LoginEndpoint = "/api/Account/Login";
    private const string GetActualEndpoint = "/api/PlatformWell/GetPlatformWellActual";
    private const string GetDummyEndpoint = "/api/PlatformWell/GetPlatformWellDummy";

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new LoginRequest { Username = username, Password = password };
            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Logging in as {Username}...", username);
            var response = await _httpClient.PostAsync(LoginEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Login failed: {StatusCode}", response.StatusCode);
                return false;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (responseBody.Trim().StartsWith("\"") || (!responseBody.Trim().StartsWith("{")))
                _bearerToken = responseBody.Trim().Trim('"');
            else
            {
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
                _bearerToken = loginResponse?.GetToken();
            }

            if (string.IsNullOrWhiteSpace(_bearerToken))
            {
                _logger.LogError("Could not extract token.");
                return false;
            }

            _logger.LogInformation("Login successful. Token acquired.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during login.");
            return false;
        }
    }

    public async Task<List<PlatformApiResponse>> GetPlatformWellActualAsync()
        => await GetPlatformWellDataAsync(GetActualEndpoint);

    public async Task<List<PlatformApiResponse>> GetPlatformWellDummyAsync()
        => await GetPlatformWellDataAsync(GetDummyEndpoint);

    private async Task<List<PlatformApiResponse>> GetPlatformWellDataAsync(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(_bearerToken))
            throw new InvalidOperationException("Not logged in.");

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _bearerToken);

            _logger.LogInformation("Calling {Endpoint}...", endpoint);
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API call failed: {StatusCode}", response.StatusCode);
                return new List<PlatformApiResponse>();
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var data = JsonConvert.DeserializeObject<List<PlatformApiResponse>>(responseBody, settings);
            _logger.LogInformation("Retrieved {Count} platforms from {Endpoint}.", data?.Count ?? 0, endpoint);
            return data ?? new List<PlatformApiResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling {Endpoint}.", endpoint);
            return new List<PlatformApiResponse>();
        }
    }
}