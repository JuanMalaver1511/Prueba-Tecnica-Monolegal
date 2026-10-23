using Monolegal.Domain.Entities;

namespace Monolegal.Application.Dtos;

public sealed record CreateInvoiceRequest(
    string ClientId,
    string Number,
    string Description,
    decimal Amount,
    string Currency,
    DateTime IssueDate,
    DateTime DueDate,
    InvoiceStatus Status);
