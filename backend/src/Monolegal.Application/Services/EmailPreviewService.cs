using Monolegal.Application.Abstractions.Email;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Dtos;
using Monolegal.Application.Reminders;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Services;

public sealed class EmailPreviewService : IEmailPreviewService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IReminderTransitionResolver _transitionResolver;
    private readonly IEmailArchive _emailArchive;

    public EmailPreviewService(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IReminderTransitionResolver transitionResolver,
        IEmailArchive emailArchive)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _transitionResolver = transitionResolver;
        _emailArchive = emailArchive;
    }

    public async Task<EmailPreviewDto?> GetByInvoiceIdAsync(
        string invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        var client = await _clientRepository.GetByIdAsync(invoice.ClientId, cancellationToken);
        var archived = await _emailArchive.GetByInvoiceIdAsync(invoiceId, cancellationToken);

        if (archived is not null)
        {
            return new EmailPreviewDto(
                invoice.Id,
                invoice.Number,
                archived.ClientName,
                archived.ToAddress,
                archived.Subject,
                archived.HtmlBody,
                archived.PlainTextBody,
                archived.SentAtUtc,
                true);
        }

        if (client is null || !invoice.Status.IsReminderPending())
        {
            return new EmailPreviewDto(
                invoice.Id,
                invoice.Number,
                client?.Name ?? "Cliente no disponible",
                client?.Email ?? string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false);
        }

        var transition = _transitionResolver.Resolve(client, invoice);

        return new EmailPreviewDto(
            invoice.Id,
            invoice.Number,
            client.Name,
            client.Email,
            transition.EmailSubject,
            transition.HtmlBody,
            transition.PlainTextBody,
            null,
            false);
    }
}
