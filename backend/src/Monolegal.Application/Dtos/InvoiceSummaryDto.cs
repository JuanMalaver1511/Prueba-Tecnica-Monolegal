namespace Monolegal.Application.Dtos;

public sealed record InvoiceSummaryDto(
    int TotalInvoices,
    decimal TotalAmount,
    string Currency,
    int PendingReminders,
    IReadOnlyList<InvoiceStatusSummaryDto> ByStatus);

public sealed record InvoiceStatusSummaryDto(
    string Status,
    string Label,
    int Count,
    decimal TotalAmount);
