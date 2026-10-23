using Monolegal.Application.Dtos;

namespace Monolegal.Application.Services;

public interface IEmailPreviewService
{
    Task<EmailPreviewDto?> GetByInvoiceIdAsync(
        string invoiceId,
        CancellationToken cancellationToken = default);
}
