namespace Monolegal.Application.Dtos;

public sealed record EmailPreviewDto(
    string InvoiceId,
    string InvoiceNumber,
    string ClientName,
    string ToAddress,
    string Subject,
    string HtmlBody,
    string PlainTextBody,
    DateTime? SentAtUtc,
    bool WasSent);
