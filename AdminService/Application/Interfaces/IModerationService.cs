using AdminService.Application.DTOs;
using AdminService.Domain.Entities;

namespace AdminService.Application.Interfaces;

public interface IModerationService
{
    Task<ModerationCase> CreateAsync(CreateModerationCaseRequest request, Guid adminUserId);

    Task<IReadOnlyList<ModerationCase>> GetAllAsync();

    Task<ModerationCase?> GetByIdAsync(Guid id);

    Task<ModerationCase?> AssignAsync(Guid id, AssignModerationCaseRequest request, Guid adminUserId);

    Task<ModerationCase?> ApproveAsync(Guid id, ModerationDecisionRequest request, Guid adminUserId);

    Task<ModerationCase?> RejectAsync(Guid id, ModerationDecisionRequest request, Guid adminUserId);

    Task<ModerationCase?> ResolveAsync(Guid id, ModerationDecisionRequest request, Guid adminUserId);
}
