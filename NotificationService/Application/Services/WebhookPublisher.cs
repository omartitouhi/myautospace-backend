using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Application.Services;

public class WebhookPublisher(
    NotificationDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    ILogger<WebhookPublisher> logger) : IWebhookPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(WebhookEventType eventType, object payload, CancellationToken cancellationToken = default)
    {
        var subscriptions = await dbContext.Webhooks
            .Where(webhook => webhook.IsActive && webhook.EventType == eventType)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            return;
        }

        var json = JsonSerializer.Serialize(payload, SerializerOptions);

        foreach (var webhook in subscriptions)
        {
            await DeliverAsync(webhook, eventType, json, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeliverAsync(Webhook webhook, WebhookEventType eventType, string json, CancellationToken cancellationToken)
    {
        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            WebhookId = webhook.Id,
            EventType = eventType,
            Payload = json,
            AttemptCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            using var client = httpClientFactory.CreateClient("webhooks");
            using var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("X-MyAutoSpace-Event", eventType.ToString());
            request.Headers.TryAddWithoutValidation("X-MyAutoSpace-Signature", ComputeSignature(webhook.Secret, json));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            using var response = await client.SendAsync(request, timeout.Token);
            delivery.ResponseStatusCode = (int)response.StatusCode;
            delivery.Success = response.IsSuccessStatusCode;
            if (!response.IsSuccessStatusCode)
            {
                delivery.LastError = $"Endpoint responded with status {(int)response.StatusCode}.";
            }
        }
        catch (Exception exception)
        {
            delivery.Success = false;
            delivery.LastError = exception.Message;
            logger.LogWarning(exception, "Webhook {WebhookId} delivery to {Url} failed.", webhook.Id, webhook.Url);
        }

        dbContext.WebhookDeliveries.Add(delivery);
    }

    private static string ComputeSignature(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret ?? string.Empty);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }
}
