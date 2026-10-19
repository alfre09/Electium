using Electium.Application.Interfaces;
using Electium.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Electium.Shared.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(to))
        {
            _logger.LogInformation("Correo no enviado (SMTP deshabilitado). Destino: {To}. Asunto: {Subject}", to, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTlsWhenAvailable);

        if (!string.IsNullOrWhiteSpace(_settings.User))
            await client.AuthenticateAsync(_settings.User, _settings.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
