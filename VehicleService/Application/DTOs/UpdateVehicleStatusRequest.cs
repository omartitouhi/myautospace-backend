using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record UpdateVehicleStatusRequest(
    [property: Required] VehicleStatus Status);
