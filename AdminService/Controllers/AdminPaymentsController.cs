using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/payments")]
public class AdminPaymentsController(
    IPaymentServiceClient paymentServiceClient,
    ICurrentAdminService currentAdminService,
    AdminDbContext dbContext) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPayments(CancellationToken cancellationToken)
    {
        try
        {
            var payments = await paymentServiceClient.GetPaymentsAsync(cancellationToken);
            return Ok(payments);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentServiceClient.GetPaymentByIdAsync(id, cancellationToken);

            return payment is null
                ? NotFound(new { message = "Payment was not found." })
                : Ok(payment);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetPaymentStats(CancellationToken cancellationToken)
    {
        try
        {
            var stats = await paymentServiceClient.GetPaymentStatsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    [HttpPost("{id:guid}/refund-request")]
    public async Task<IActionResult> RequestRefund(Guid id, CancellationToken cancellationToken)
    {
        var adminUserId = currentAdminService.GetAdminUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new { message = "Admin user id claim is missing or invalid." });
        }

        await AddActionLogAsync(adminUserId.Value, id, cancellationToken);

        try
        {
            await paymentServiceClient.RequestRefundAsync(id, adminUserId.Value, cancellationToken);
            return Ok(new { message = "Refund request submitted successfully." });
        }
        catch (NotImplementedException exception)
        {
            return NotImplemented(exception.Message);
        }
    }

    private async Task AddActionLogAsync(Guid adminUserId, Guid paymentId, CancellationToken cancellationToken)
    {
        dbContext.AdminActionLogs.Add(new AdminActionLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = "RequestPaymentRefund",
            TargetService = "PaymentService",
            TargetEntity = "Payment",
            TargetEntityId = paymentId,
            Description = $"Refund request submitted for payment {paymentId}.",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private ObjectResult NotImplemented(string message)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message,
            targetService = "PaymentService"
        });
    }
}
