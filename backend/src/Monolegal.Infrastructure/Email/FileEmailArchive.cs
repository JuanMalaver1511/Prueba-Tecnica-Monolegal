using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Email;

namespace Monolegal.Infrastructure.Email;

public sealed class FileEmailArchive : IEmailArchive
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly string _archiveDirectory;
    private readonly ILogger<FileEmailArchive> _logger;

    public FileEmailArchive(
        IOptions<EmailSettings> settings,
        string contentRootPath,
        ILogger<FileEmailArchive> logger)
    {
        _archiveDirectory = EmailOutboxPath.Resolve(settings.Value, contentRootPath, "archive");
        _logger = logger;
    }

    public async Task SaveAsync(ArchivedEmail email, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_archiveDirectory);

        var filePath = GetFilePath(email.InvoiceId);

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, email, SerializerOptions, cancellationToken);

        _logger.LogInformation(
            "Vista previa del correo de la factura {InvoiceNumber} archivada en {FilePath}.",
            email.InvoiceNumber,
            filePath);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(_archiveDirectory))
        {
            Directory.Delete(_archiveDirectory, recursive: true);
            _logger.LogInformation("Archivo de correos demo limpiado en {Directory}.", _archiveDirectory);
        }

        return Task.CompletedTask;
    }

    public async Task<ArchivedEmail?> GetByInvoiceIdAsync(
        string invoiceId,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(invoiceId);

        if (!File.Exists(filePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<ArchivedEmail>(
            stream,
            SerializerOptions,
            cancellationToken);
    }

    private string GetFilePath(string invoiceId)
    {
        var safeId = new string(invoiceId
            .Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character)
            .ToArray());

        return Path.Combine(_archiveDirectory, $"{safeId}.json");
    }
}
