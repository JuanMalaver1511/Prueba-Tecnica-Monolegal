namespace Monolegal.Application.Abstractions.Email;

public sealed record EmailMessage(
    string ToAddress,
    string ToName,
    string Subject,
    string HtmlBody,
    string PlainTextBody);
