namespace Monolegal.Domain.Entities;

public sealed class Invoice
{
    public string Id { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = "COP";

    public DateTime IssueDate { get; init; }

    public DateTime DueDate { get; init; }

    public InvoiceStatus Status { get; init; }
}
