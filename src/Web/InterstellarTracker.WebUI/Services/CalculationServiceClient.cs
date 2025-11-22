using InterstellarTracker.WebUI.Models;

namespace InterstellarTracker.WebUI.Services;

/// <summary>
/// HTTP client service for interacting with the Calculation Service API.
/// </summary>
public class CalculationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalculationServiceClient> _logger;

    public CalculationServiceClient(HttpClient httpClient, ILogger<CalculationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all celestial bodies.
    /// </summary>
    public async Task<List<CelestialBodyDto>> GetAllCelestialBodiesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all celestial bodies from Calculation Service");
            var response = await _httpClient.GetFromJsonAsync<List<CelestialBodyDto>>("api/celestialbodies");
            return response ?? new List<CelestialBodyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching celestial bodies");
            return new List<CelestialBodyDto>();
        }
    }

    /// <summary>
    /// Get position of a specific celestial body.
    /// </summary>
    public async Task<CelestialBodyPositionDto?> GetCelestialBodyPositionAsync(string id, DateTime? date = null)
    {
        try
        {
            var query = date.HasValue ? $"?date={date.Value:O}" : "";
            _logger.LogInformation("Fetching position for body {BodyId}", id);
            return await _httpClient.GetFromJsonAsync<CelestialBodyPositionDto>($"api/celestialbodies/{id}/position{query}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching position for body {BodyId}", id);
            return null;
        }
    }

    /// <summary>
    /// Get all interstellar objects.
    /// </summary>
    public async Task<List<InterstellarObjectDto>> GetAllInterstellarObjectsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all interstellar objects from Calculation Service");
            var response = await _httpClient.GetFromJsonAsync<List<InterstellarObjectDto>>("api/interstellarobjects");
            return response ?? new List<InterstellarObjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching interstellar objects");
            return new List<InterstellarObjectDto>();
        }
    }

    /// <summary>
    /// Get position of a specific interstellar object.
    /// </summary>
    public async Task<InterstellarObjectPositionDto?> GetInterstellarObjectPositionAsync(string id, DateTime? date = null)
    {
        try
        {
            var query = date.HasValue ? $"?date={date.Value:O}" : "";
            _logger.LogInformation("Fetching position for interstellar object {ObjectId}", id);
            return await _httpClient.GetFromJsonAsync<InterstellarObjectPositionDto>($"api/interstellarobjects/{id}{query}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching position for interstellar object {ObjectId}", id);
            return null;
        }
    }
}
