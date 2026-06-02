namespace NotificationService.Domain.Constants;

public static class NotificationPolicies
{
    /// <summary>Any authenticated marketplace user.</summary>
    public const string AuthenticatedUser = "AuthenticatedUser";

    /// <summary>Sending notifications to arbitrary recipients is restricted to admins / back-office.</summary>
    public const string Sender = "Sender";

    public const string Admin = "Admin";
}
