namespace Monolegal.Application.Dtos;

public sealed record CreateClientWithInvoiceRequest(
    CreateClientRequest Client,
    CreateInvoiceRequest Invoice);
