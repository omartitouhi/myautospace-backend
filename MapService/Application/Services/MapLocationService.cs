using Microsoft.EntityFrameworkCore;
using MapService.Application.DTOs;
using MapService.Application.Interfaces;
using MapService.Domain.Entities;
using MapService.Domain.Enums;
using MapService.Infrastructure.Data;

namespace MapService.Application.Services;

public class MapLocationService(MapDbContext dbContext) : IMapLocationService
{
    private const double EarthRadiusKm = 6371.0;
    private const double KmPerDegreeLatitude = 111.0;

    public async Task<MapLocationResponse> CreateAsync(CreateLocationRequest request, Guid ownerAuthUserId)
    {
        ValidateCoordinates(request.Latitude, request.Longitude);
        ValidateStringNotEmpty(request.City, nameof(request.City));
        ValidateStringNotEmpty(request.Country, nameof(request.Country));

        var existing = await dbContext.MapLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.EntityId == request.EntityId && l.EntityType == request.EntityType);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"A location already exists for entity '{request.EntityId}' of type '{request.EntityType}'. Use PUT to update it.");
        }

        var location = new MapLocation
        {
            Id = Guid.NewGuid(),
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address?.Trim(),
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            OwnerAuthUserId = ownerAuthUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.MapLocations.Add(location);
        await dbContext.SaveChangesAsync();

        return MapLocationResponse.FromEntity(location);
    }

    public async Task<MapLocationResponse> GetByIdAsync(Guid id)
    {
        var location = await dbContext.MapLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Location '{id}' was not found.");

        return MapLocationResponse.FromEntity(location);
    }

    public async Task<MapLocationResponse> GetByEntityAsync(Guid entityId, EntityType entityType)
    {
        var location = await dbContext.MapLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.EntityId == entityId && l.EntityType == entityType)
            ?? throw new KeyNotFoundException(
                $"No location found for entity '{entityId}' of type '{entityType}'.");

        return MapLocationResponse.FromEntity(location);
    }

    public async Task<IReadOnlyCollection<MapLocationResponse>> GetByOwnerAsync(Guid ownerAuthUserId)
    {
        var locations = await dbContext.MapLocations
            .AsNoTracking()
            .Where(l => l.OwnerAuthUserId == ownerAuthUserId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return locations.Select(MapLocationResponse.FromEntity).ToList();
    }

    public async Task<MapLocationResponse> UpdateAsync(Guid id, UpdateLocationRequest request, Guid ownerAuthUserId)
    {
        var location = await GetLocationAndEnsureOwnershipAsync(id, ownerAuthUserId);

        if (request.Latitude.HasValue || request.Longitude.HasValue)
        {
            var newLat = request.Latitude ?? location.Latitude;
            var newLon = request.Longitude ?? location.Longitude;
            ValidateCoordinates(newLat, newLon);
            location.Latitude = newLat;
            location.Longitude = newLon;
        }

        if (request.City is not null) ValidateStringNotEmpty(request.City, nameof(request.City));
        if (request.Country is not null) ValidateStringNotEmpty(request.Country, nameof(request.Country));

        if (request.Address is not null) location.Address = request.Address.Trim();
        if (request.City is not null) location.City = request.City.Trim();
        if (request.Country is not null) location.Country = request.Country.Trim();
        if (request.IsActive.HasValue) location.IsActive = request.IsActive.Value;
        location.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return MapLocationResponse.FromEntity(location);
    }

    public async Task DeleteAsync(Guid id, Guid ownerAuthUserId)
    {
        var location = await GetLocationAndEnsureOwnershipAsync(id, ownerAuthUserId);

        dbContext.MapLocations.Remove(location);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<NearbyLocationResult>> GetNearbyAsync(NearbySearchRequest request)
    {
        ValidateCoordinates(request.Latitude, request.Longitude);

        // Bounding box pre-filter in SQL (same approach as SearchService)
        var deltaLat = request.RadiusKm / KmPerDegreeLatitude;
        var cosLat = Math.Cos(DegreesToRadians(request.Latitude));
        var deltaLon = Math.Abs(cosLat) < 1e-6
            ? 180.0
            : request.RadiusKm / (KmPerDegreeLatitude * Math.Abs(cosLat));

        var minLat = request.Latitude - deltaLat;
        var maxLat = request.Latitude + deltaLat;
        var minLon = request.Longitude - deltaLon;
        var maxLon = request.Longitude + deltaLon;

        var query = dbContext.MapLocations
            .AsNoTracking()
            .Where(l => l.IsActive
                && l.Latitude >= minLat && l.Latitude <= maxLat
                && l.Longitude >= minLon && l.Longitude <= maxLon);

        if (request.EntityType.HasValue)
        {
            query = query.Where(l => l.EntityType == request.EntityType.Value);
        }

        var candidates = await query.ToListAsync();

        // Precise Haversine distance filter and sorting in memory
        var results = candidates
            .Select(l => (location: l, distance: Haversine(request.Latitude, request.Longitude, l.Latitude, l.Longitude)))
            .Where(x => x.distance <= request.RadiusKm)
            .OrderBy(x => x.distance)
            .Take(request.Limit)
            .Select(x => NearbyLocationResult.FromEntity(x.location, x.distance))
            .ToList();

        return results;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<MapLocation> GetLocationAndEnsureOwnershipAsync(Guid id, Guid ownerAuthUserId)
    {
        var location = await dbContext.MapLocations
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Location '{id}' was not found.");

        if (location.OwnerAuthUserId != ownerAuthUserId)
        {
            throw new UnauthorizedAccessException("You do not own this location.");
        }

        return location;
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }
    }

    private static void ValidateStringNotEmpty(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{fieldName}' cannot be empty or whitespace.", fieldName);
        }
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return EarthRadiusKm * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
