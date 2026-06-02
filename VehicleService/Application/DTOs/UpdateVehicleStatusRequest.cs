using System.ComponentModel.DataAnnotations;
using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs;

public record UpdateVehicleStatusRequest(
    [Required] VehicleStatus Status);
