using Microsoft.Extensions.Logging;
using Monolegal.Application.Abstractions.Email;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Dtos;
using Monolegal.Application.Reminders;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Services;

public sealed class ReminderProcessingService : IReminderProcessingService
{
    private readonly IClientRepository _clientRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IReminderTransitionResolver _transitionResolver;
    private readonly IEmailSender _emailSender;
    private readonly IEmailArchive _emailArchive;
    private readonly ILogger<ReminderProcessingService> _logger;

    public ReminderProcessingService(
        IClientRepository clientRepository,
        IInvoiceRepository invoiceRepository,
        IReminderTransitionResolver transitionResolver,
        IEmailSender emailSender,
        IEmailArchive emailArchive,
        ILogger<ReminderProcessingService> logger)
    {
        _clientRepository = clientRepository;
        _invoiceRepository = invoiceRepository;
        _transitionResolver = transitionResolver;
        _emailSender = emailSender;
        _emailArchive = emailArchive;
        _logger = logger;
    }

    public async Task<ReminderProcessingResult> ProcessAsync(
        IReadOnlyCollection<string>? clientIds = null,
        CancellationToken cancellationToken = default)
    {
        var clients = await LoadClientsAsync(clientIds, cancellationToken);
        var actions = new List<ReminderActionDto>();

        var invoicesEvaluated = 0;
        var eligibleInvoices = 0;
        var emailsSent = 0;
        var invoicesUpdated = 0;
        var failures = 0;

        foreach (var client in clients)
        {
            var invoices = await _invoiceRepository.GetByClientIdAsync(client.Id, cancellationToken);

            foreach (var invoice in invoices)
            {
                invoicesEvaluated++;

                if (!invoice.Status.IsReminderPending())
                {
                    continue;
                }

                eligibleInvoices++;
                var transition = _transitionResolver.Resolve(client, invoice);

                try
                {
                    await _emailSender.SendAsync(
                        new EmailMessage(
                            client.Email,
                            client.Name,
                            transition.EmailSubject,
                            transition.HtmlBody,
                            transition.PlainTextBody),
                        cancellationToken);

                    emailsSent++;

                    await _emailArchive.SaveAsync(
                        new ArchivedEmail(
                            invoice.Id,
                            invoice.Number,
                            client.Name,
                            client.Email,
                            transition.EmailSubject,
                            transition.HtmlBody,
                            transition.PlainTextBody,
                            DateTime.UtcNow),
                        cancellationToken);

                    var updated = await _invoiceRepository.UpdateStatusAsync(
                        invoice.Id,
                        invoice.Status,
                        transition.ToStatus,
                        cancellationToken);

                    if (!updated)
                    {
                        failures++;
                        var message = "La factura cambió de estado antes de aplicar la actualización.";
                        _logger.LogWarning(
                            "No se actualizó la factura {InvoiceNumber}: {Reason}",
                            invoice.Number,
                            message);

                        actions.Add(CreateAction(client, invoice, transition, false, message));
                        continue;
                    }

                    invoicesUpdated++;
                    actions.Add(CreateAction(client, invoice, transition, true, null));

                    _logger.LogInformation(
                        "Recordatorio {FromStatus} -> {ToStatus} enviado a {ClientEmail} para la factura {InvoiceNumber}.",
                        invoice.Status.ToDbValue(),
                        transition.ToStatus.ToDbValue(),
                        client.Email,
                        invoice.Number);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    failures++;
                    _logger.LogError(
                        exception,
                        "No fue posible enviar o registrar el recordatorio de la factura {InvoiceNumber} del cliente {ClientId}.",
                        invoice.Number,
                        client.Id);

                    actions.Add(CreateAction(client, invoice, transition, false, exception.Message));
                }
            }
        }

        return new ReminderProcessingResult(
            clients.Count,
            invoicesEvaluated,
            invoicesEvaluated - eligibleInvoices,
            eligibleInvoices,
            emailsSent,
            invoicesUpdated,
            failures,
            actions);
    }

    private Task<IReadOnlyList<Client>> LoadClientsAsync(
        IReadOnlyCollection<string>? clientIds,
        CancellationToken cancellationToken)
    {
        if (clientIds is null || clientIds.Count == 0)
        {
            return _clientRepository.GetAllAsync(cancellationToken);
        }

        var distinctIds = clientIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return _clientRepository.GetByIdsAsync(distinctIds, cancellationToken);
    }

    private static ReminderActionDto CreateAction(
        Client client,
        Invoice invoice,
        ReminderTransition transition,
        bool succeeded,
        string? errorMessage) =>
        new(
            invoice.Id,
            invoice.Number,
            client.Id,
            client.Name,
            client.Email,
            invoice.Status.ToDbValue(),
            transition.ToStatus.ToDbValue(),
            transition.EmailSubject,
            succeeded,
            errorMessage);
}
