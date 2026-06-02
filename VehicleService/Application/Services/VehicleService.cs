using Microsoft.EntityFrameworkCore;
using VehicleService.Application.DTOs;
using VehicleService.Application.Interfaces;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Application.Services;

public class VehicleService(VehicleDbContext dbContext) : IVehicleService
{
    private static readonly int MinYear = 1900;
    private static readonly int MaxYear = DateTime.UtcNow.Year + 1;

    public async Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, Guid ownerAuthUserId)
    {
        ValidateStringNotEmpty(request.Make, nameof(request.Make));
        ValidateStringNotEmpty(request.Model, nameof(request.Model));
        ValidateStringNotEmpty(request.Country, nameof(request.Country));
        ValidateStringNotEmpty(request.City, nameof(request.City));
        ValidateYear(request.Year);
        ValidatePrice(request.Price);
        ValidateMileage(request.Mileage);

        if (request.VIN is not null)
        {
            await EnsureVinIsUniqueAsync(request.VIN, excludeId: null);
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            OwnerAuthUserId = ownerAuthUserId,
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            VIN = request.VIN?.Trim().ToUpperInvariant(),
            Price = request.Price,
            Description = request.Description?.Trim(),
            Mileage = request.Mileage,
            FuelType = request.FuelType,
            Transmission = request.Transmission,
            BodyType = request.BodyType,
            ListingType = request.ListingType,
            Status = VehicleStatus.Draft,
            Color = request.Color?.Trim(),
            Country = request.Country.Trim(),
            City = request.City.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Vehicles.Add(vehicle);
        await dbContext.SaveChangesAsync();

        return VehicleResponse.FromEntity(vehicle);
    }

    public async Task<VehicleResponse> GetByIdAsync(Guid id)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vehicle '{id}' was not found.");

        return VehicleResponse.FromEntity(vehicle);
    }

    public async Task<IReadOnlyCollection<VehicleSummaryResponse>> GetAllActiveAsync()
    {
        var vehicles = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Status == VehicleStatus.Active)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return vehicles.Select(VehicleSummaryResponse.FromEntity).ToList();
    }

    public async Task<IReadOnlyCollection<VehicleSummaryResponse>> GetByOwnerAsync(Guid ownerAuthUserId)
    {
        var vehicles = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.OwnerAuthUserId == ownerAuthUserId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return vehicles.Select(VehicleSummaryResponse.FromEntity).ToList();
    }

    public async Task<VehicleResponse> UpdateAsync(Guid id, UpdateVehicleRequest request, Guid ownerAuthUserId)
    {
        var vehicle = await GetVehicleAndEnsureOwnershipAsync(id, ownerAuthUserId);

        EnsureVehicleIsEditable(vehicle);

        if (request.Make is not null) ValidateStringNotEmpty(request.Make, nameof(request.Make));
        if (request.Model is not null) ValidateStringNotEmpty(request.Model, nameof(request.Model));
        if (request.Country is not null) ValidateStringNotEmpty(request.Country, nameof(request.Country));
        if (request.City is not null) ValidateStringNotEmpty(request.City, nameof(request.City));

        if (request.Year.HasValue)
        {
            ValidateYear(request.Year.Value);
        }

        if (request.Price.HasValue)
        {
            ValidatePrice(request.Price.Value);
        }

        if (request.Mileage.HasValue)
        {
            ValidateMileage(request.Mileage.Value);
        }

        if (request.VIN is not null)
        {
            await EnsureVinIsUniqueAsync(request.VIN, excludeId: id);
        }

        if (request.Make is not null) vehicle.Make = request.Make.Trim();
        if (request.Model is not null) vehicle.Model = request.Model.Trim();
        if (request.Year.HasValue) vehicle.Year = request.Year.Value;
        if (request.VIN is not null) vehicle.VIN = request.VIN.Trim().ToUpperInvariant();
        if (request.Price.HasValue) vehicle.Price = request.Price.Value;
        if (request.Description is not null) vehicle.Description = request.Description.Trim();
        if (request.Mileage.HasValue) vehicle.Mileage = request.Mileage.Value;
        if (request.FuelType.HasValue) vehicle.FuelType = request.FuelType.Value;
        if (request.Transmission.HasValue) vehicle.Transmission = request.Transmission.Value;
        if (request.BodyType.HasValue) vehicle.BodyType = request.BodyType.Value;
        if (request.Color is not null) vehicle.Color = request.Color.Trim();
        if (request.Country is not null) vehicle.Country = request.Country.Trim();
        if (request.City is not null) vehicle.City = request.City.Trim();

        vehicle.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return VehicleResponse.FromEntity(vehicle);
    }

    public async Task UpdateStatusAsync(Guid id, UpdateVehicleStatusRequest request, Guid ownerAuthUserId)
    {
        var vehicle = await GetVehicleAndEnsureOwnershipAsync(id, ownerAuthUserId);

        ValidateStatusTransition(vehicle.Status, request.Status);

        vehicle.Status = request.Status;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid ownerAuthUserId)
    {
        var vehicle = await GetVehicleAndEnsureOwnershipAsync(id, ownerAuthUserId);

        if (vehicle.Status == VehicleStatus.Sold || vehicle.Status == VehicleStatus.Rented)
        {
            throw new InvalidOperationException(
                $"Cannot delete a vehicle with status '{vehicle.Status}'. Change the status first.");
        }

        vehicle.IsDeleted = true;
        vehicle.DeletedAt = DateTime.UtcNow;
        vehicle.Status = VehicleStatus.Inactive;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<Vehicle> GetVehicleAndEnsureOwnershipAsync(Guid id, Guid ownerAuthUserId)
    {
        var vehicle = await dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vehicle '{id}' was not found.");

        if (vehicle.OwnerAuthUserId != ownerAuthUserId)
        {
            throw new UnauthorizedAccessException("You are not allowed to modify this vehicle.");
        }

        return vehicle;
    }

    private async Task EnsureVinIsUniqueAsync(string vin, Guid? excludeId)
    {
        var normalizedVin = vin.Trim().ToUpperInvariant();

        var exists = await dbContext.Vehicles
            .IgnoreQueryFilters()
            .AnyAsync(v => v.VIN == normalizedVin && v.Id != excludeId);

        if (exists)
        {
            throw new InvalidOperationException($"A vehicle with VIN '{normalizedVin}' already exists.");
        }
    }

    private static void ValidateYear(int year)
    {
        if (year < MinYear || year > MaxYear)
        {
            throw new ArgumentOutOfRangeException(nameof(year),
                $"Year must be between {MinYear} and {MaxYear}.");
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
        }
    }

    private static void ValidateMileage(int mileage)
    {
        if (mileage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mileage), "Mileage cannot be negative.");
        }
    }

    private static void EnsureVehicleIsEditable(Vehicle vehicle)
    {
        if (vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("A sold vehicle cannot be edited. Change its status first.");
        }
    }

    private static void ValidateStringNotEmpty(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"'{fieldName}' cannot be empty or whitespace.", fieldName);
        }
    }

    private static void ValidateStatusTransition(VehicleStatus current, VehicleStatus requested)
    {
        var forbidden = (current, requested) switch
        {
            (VehicleStatus.Sold, _)     when requested != VehicleStatus.Inactive => true,
            (VehicleStatus.Rented, _)   when requested != VehicleStatus.Active
                                          && requested != VehicleStatus.Inactive   => true,
            _ => false
        };

        if (forbidden)
        {
            throw new InvalidOperationException(
                $"Cannot transition from '{current}' to '{requested}'.");
        }
    }
}
