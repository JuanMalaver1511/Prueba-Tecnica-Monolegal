namespace Monolegal.Application.Dtos;

public sealed record ReminderProcessingResult(
    int ClientsProcessed,
    int InvoicesEvaluated,
    int InvoicesSkipped,
    int EligibleInvoices,
    int EmailsSent,
    int InvoicesUpdated,
    int Failures,
    IReadOnlyList<ReminderActionDto> Actions)
{
    public static ReminderProcessingResult Empty { get; } = new(
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        Array.Empty<ReminderActionDto>());
}

public sealed record ReminderActionDto(
    string InvoiceId,
    string InvoiceNumber,
    string ClientId,
    string ClientName,
    string ClientEmail,
    string PreviousStatus,
    string NewStatus,
    string EmailSubject,
    bool Succeeded,
    string? ErrorMessage);
