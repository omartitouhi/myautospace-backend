using AdminService.Application.DTOs;
using AdminService.Domain.Entities;

namespace AdminService.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<AdminReport>> GetAllAsync();

    Task<AdminReport?> GetByIdAsync(Guid id);

    Task<AdminReport> GenerateAsync(GenerateReportRequest request, Guid adminUserId);
}
