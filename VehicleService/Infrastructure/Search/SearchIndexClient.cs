using System.Net;
using System.Net.Http.Headers;
using VehicleService.Application.Interfaces;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;

namespace VehicleService.Infrastructure.Search;

/// <summary>
/// Pushes vehicle documents into SearchService's index
/// (PUT /api/search/index, DELETE /api/search/index/{externalId}).
/// The caller's bearer token is forwarded as-is — vehicle mutations are only
/// reachable by sellers, and the Seller role satisfies SearchService's
/// IndexManager policy.
/// </summary>
public class SearchIndexClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SearchIndexClient> logger) : ISearchIndexClient
{
    public async Task UpsertAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            ExternalId = vehicle.Id,
            Type = "Vehicle",
            Title = $"{vehicle.Make} {vehicle.Model} {vehicle.Year}",
            Description = vehicle.Description ?? string.Empty,
            Category = vehicle.BodyType.ToString(),
            Brand = vehicle.Make,
            City = vehicle.City,
            Country = vehicle.Country,
            Latitude = (double?)null,
            Longitude = (double?)null,
            Price = (decimal?)vehicle.Price,
            Currency = "TND",
            Tags = BuildTags(vehicle),
            Popularity = (int?)null,
            IsActive = vehicle.Status == VehicleStatus.Active
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, "api/search/index")
        {
            Content = JsonContent.Create(payload)
        };

        await SendAsync(request, $"index vehicle '{vehicle.Id}'", allowNotFound: false, cancellationToken);
    }

    public async Task RemoveAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/search/index/{vehicleId}");

        // Removing a document that was never indexed is fine.
        await SendAsync(request, $"remove vehicle '{vehicleId}' from index", allowNotFound: true, cancellationToken);
    }

    private async Task SendAsync(
        HttpRequestMessage request,
        string operation,
        bool allowNotFound,
        CancellationToken cancellationToken)
    {
        var bearer = GetCallerBearerToken();
        if (bearer is null)
        {
            logger.LogWarning("Skipped search index call ({Operation}): no bearer token on the current request.", operation);
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode && !(allowNotFound && response.StatusCode == HttpStatusCode.NotFound))
            {
                logger.LogWarning(
                    "Search index call failed ({Operation}): {StatusCode}.",
                    operation,
                    (int)response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Search index call failed ({Operation}).", operation);
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

    private static string BuildTags(Vehicle vehicle)
    {
        var tags = new List<string>
        {
            vehicle.FuelType.ToString(),
            vehicle.Transmission.ToString(),
            vehicle.ListingType.ToString(),
            vehicle.Year.ToString()
        };

        if (!string.IsNullOrWhiteSpace(vehicle.Color))
        {
            tags.Add(vehicle.Color);
        }

        return string.Join(' ', tags);
    }
}
