using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Dtos;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Services;

public sealed class InvoiceQueryService : IInvoiceQueryService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;

    public InvoiceQueryService(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(
        string? clientId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await LoadInvoicesAsync(clientId, cancellationToken);
        var clients = await _clientRepository.GetAllAsync(cancellationToken);
        var clientNames = clients.ToDictionary(client => client.Id, client => client.Name);

        return invoices
            .OrderBy(invoice => invoice.DueDate)
            .ThenBy(invoice => invoice.Number)
            .Select(invoice => new InvoiceDto(
                invoice.Id,
                invoice.ClientId,
                clientNames.GetValueOrDefault(invoice.ClientId, "Cliente no disponible"),
                invoice.Number,
                invoice.Description,
                invoice.Amount,
                invoice.Currency,
                invoice.IssueDate,
                invoice.DueDate,
                invoice.Status.ToDbValue(),
                invoice.Status.ToDisplayName(),
                invoice.Status.IsReminderPending()))
            .ToList();
    }

    public async Task<InvoiceSummaryDto> GetSummaryAsync(
        string? clientId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await LoadInvoicesAsync(clientId, cancellationToken);
        var currencies = invoices.Select(invoice => invoice.Currency).Distinct().ToList();
        var currency = currencies.Count == 1 ? currencies[0] : "COP";

        var byStatus = Enum.GetValues<InvoiceStatus>()
            .Select(status =>
            {
                var invoicesInStatus = invoices.Where(invoice => invoice.Status == status).ToList();

                return new InvoiceStatusSummaryDto(
                    status.ToDbValue(),
                    status.ToDisplayName(),
                    invoicesInStatus.Count,
                    invoicesInStatus.Sum(invoice => invoice.Amount));
            })
            .ToList();

        return new InvoiceSummaryDto(
            invoices.Count,
            invoices.Sum(invoice => invoice.Amount),
            currency,
            invoices.Count(invoice => invoice.Status.IsReminderPending()),
            byStatus);
    }

    private Task<IReadOnlyList<Invoice>> LoadInvoicesAsync(
        string? clientId,
        CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(clientId)
            ? _invoiceRepository.GetAllAsync(cancellationToken)
            : _invoiceRepository.GetByClientIdAsync(clientId, cancellationToken);
}
