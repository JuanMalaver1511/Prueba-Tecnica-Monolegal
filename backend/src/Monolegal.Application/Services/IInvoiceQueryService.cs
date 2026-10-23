using Monolegal.Application.Dtos;

namespace Monolegal.Application.Services;

public interface IInvoiceQueryService
{
    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(
        string? clientId,
        CancellationToken cancellationToken = default);

    Task<InvoiceSummaryDto> GetSummaryAsync(
        string? clientId,
        CancellationToken cancellationToken = default);
}
