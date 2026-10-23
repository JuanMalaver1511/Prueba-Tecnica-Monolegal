namespace Monolegal.Application.Dtos;

public sealed record InvoiceDto(
    string Id,
    string ClientId,
    string ClientName,
    string Number,
    string Description,
    decimal Amount,
    string Currency,
    DateTime IssueDate,
    DateTime DueDate,
    string Status,
    string StatusLabel,
    bool IsReminderPending);
