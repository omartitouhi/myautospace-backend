using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BookingService.Application.Interfaces;

namespace BookingService.Infrastructure.Vehicles;

/// <summary>
/// Reads a vehicle from VehicleService (GET /api/vehicles/{id}), forwarding the
/// caller's bearer token so the call satisfies VehicleService's authenticated
/// policy. Base URL from `VehicleService:BaseUrl` (dev default localhost:5155,
/// docker http://vehicleservice:8080).
/// </summary>
public class VehicleLookupClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<VehicleLookupClient> logger) : IVehicleLookupClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<VehicleInfo?> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var bearer = GetCallerBearerToken();
        if (bearer is null)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/vehicles/{vehicleId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Vehicle lookup failed for '{VehicleId}': {StatusCode}.", vehicleId, (int)response.StatusCode);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<VehicleResponseDto>(JsonOptions, cancellationToken);
            if (dto is null)
            {
                return null;
            }

            return new VehicleInfo(
                dto.Id,
                dto.OwnerAuthUserId,
                dto.Make,
                dto.Model,
                dto.Year,
                dto.City,
                dto.Country,
                dto.Status,
                dto.ListingType,
                dto.Price);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Vehicle lookup failed for '{VehicleId}'.", vehicleId);
            return null;
        }
    }

    private string? GetCallerBearerToken()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        return header is not null && header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? header[prefix.Length..]
            : null;
    }

    // Subset of VehicleService's VehicleResponse (camelCase JSON, enums as strings).
    private sealed record VehicleResponseDto(
        Guid Id,
        Guid OwnerAuthUserId,
        string Make,
        string Model,
        int Year,
        decimal Price,
        string City,
        string Country,
        string Status,
        string ListingType);
}
