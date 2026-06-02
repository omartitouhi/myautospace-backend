using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/reports")]
public class ReportsController(
    IReportService reportService,
    ICurrentAdminService currentAdminService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminReportResponse>>> GetReports()
    {
        var reports = await reportService.GetAllAsync();
        return Ok(reports.Select(ToResponse).ToList());
    }

    [HttpPost("generate")]
    public async Task<ActionResult<AdminReportResponse>> Generate(GenerateReportRequest request)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        try
        {
            var report = await reportService.GenerateAsync(request, adminUserId.Value);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, ToResponse(report));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminReportResponse>> GetById(Guid id)
    {
        var report = await reportService.GetByIdAsync(id);

        return report is null
            ? NotFound(new { message = "Report was not found." })
            : Ok(ToResponse(report));
    }

    private static AdminReportResponse ToResponse(AdminReport report)
    {
        return new AdminReportResponse(
            report.Id,
            report.ReportType,
            report.Title,
            report.Description,
            report.GeneratedByAdminId,
            report.GeneratedAt,
            report.DataJson);
    }
}
