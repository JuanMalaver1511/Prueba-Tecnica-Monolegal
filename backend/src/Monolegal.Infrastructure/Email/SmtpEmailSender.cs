using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Email;

namespace Monolegal.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Smtp.Host))
        {
            throw new InvalidOperationException("El modo SMTP está activo, pero Email:Smtp:Host no está configurado.");
        }

        var secureSocketOptions = ParseSecureSocketOptions(_settings.Smtp.SecureSocketOptions);
        var mimeMessage = MimeMessageFactory.Create(message, _settings);

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.Smtp.Host,
            _settings.Smtp.Port,
            secureSocketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.Smtp.UserName))
        {
            await client.AuthenticateAsync(
                _settings.Smtp.UserName,
                _settings.Smtp.Password,
                cancellationToken);
        }

        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Correo enviado por SMTP a {ToAddress}.", message.ToAddress);
    }

    private static SecureSocketOptions ParseSecureSocketOptions(string value) =>
        Enum.TryParse<SecureSocketOptions>(value, ignoreCase: true, out var options)
            ? options
            : throw new InvalidOperationException(
                $"Valor inválido para Email:Smtp:SecureSocketOptions: '{value}'.");
}
