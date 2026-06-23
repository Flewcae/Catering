namespace Catering.NotificationService.Infrastructure.Channels;

public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; set; } = string.Empty;
    public string CredentialsJson { get; set; } = string.Empty;
}
