namespace Monolegal.Application.Abstractions.Email;

public sealed record ArchivedEmail(
    string InvoiceId,
    string InvoiceNumber,
    string ClientName,
    string ToAddress,
    string Subject,
    string HtmlBody,
    string PlainTextBody,
    DateTime SentAtUtc);

public interface IEmailArchive
{
    Task SaveAsync(ArchivedEmail email, CancellationToken cancellationToken = default);

    Task<ArchivedEmail?> GetByInvoiceIdAsync(
        string invoiceId,
        CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
