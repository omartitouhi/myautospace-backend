namespace AdminService.Domain.Entities;

public class AdminReport
{
    public Guid Id { get; set; }

    public string ReportType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid GeneratedByAdminId { get; set; }

    public DateTime GeneratedAt { get; set; }

    public string DataJson { get; set; } = string.Empty;
}
