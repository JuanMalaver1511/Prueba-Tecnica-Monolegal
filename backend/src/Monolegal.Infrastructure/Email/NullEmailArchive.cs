using Monolegal.Application.Abstractions.Email;

namespace Monolegal.Infrastructure.Email;

public sealed class NullEmailArchive : IEmailArchive
{
    public Task SaveAsync(ArchivedEmail email, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<ArchivedEmail?> GetByInvoiceIdAsync(
        string invoiceId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<ArchivedEmail?>(null);

    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
