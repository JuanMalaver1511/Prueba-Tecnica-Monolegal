using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Email;

namespace Monolegal.Infrastructure.Email;

public sealed class FileEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly string _outputDirectory;
    private readonly ILogger<FileEmailSender> _logger;

    public FileEmailSender(
        IOptions<EmailSettings> settings,
        string contentRootPath,
        ILogger<FileEmailSender> logger)
    {
        _settings = settings.Value;
        _outputDirectory = EmailOutboxPath.Resolve(_settings, contentRootPath);
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_outputDirectory);

        var mimeMessage = MimeMessageFactory.Create(message, _settings);
        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{SanitizeFileName(message.Subject)}.eml";
        var filePath = Path.Combine(_outputDirectory, fileName);

        await using (var stream = File.Create(filePath))
        {
            await mimeMessage.WriteToAsync(stream, cancellationToken);
        }

        _logger.LogInformation(
            "Correo para {ToAddress} guardado en {FilePath} (modo File).",
            message.ToAddress,
            filePath);
    }

    private static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character =>
            invalidCharacters.Contains(character) ? '-' : character).ToArray());

        return sanitized.Length > 90 ? sanitized[..90] : sanitized;
    }
}
