using Catering.NotificationService.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Catering.NotificationService.Infrastructure.Channels;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        var secureSocketOptions = _options.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : _options.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("[Email] Sent to {Recipient} via {Host}:{Port}", recipient, _options.Host, _options.Port);
    }
}
