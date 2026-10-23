using Monolegal.Domain.Entities;

namespace Monolegal.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Invoice?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invoice>> GetByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        string invoiceId,
        InvoiceStatus expectedStatus,
        InvoiceStatus newStatus,
        CancellationToken cancellationToken = default);
}
