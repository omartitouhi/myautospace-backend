using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public record SubscribePackRequest(
    PackType PackType,
    DateTime? StartDate,
    DateTime? EndDate);
