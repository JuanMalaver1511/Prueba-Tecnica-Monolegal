using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

public sealed record ReminderTransition(
    InvoiceStatus FromStatus,
    InvoiceStatus ToStatus,
    string EmailSubject,
    string HtmlBody,
    string PlainTextBody);
