using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Monolegal.Application.Abstractions.Email;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Reminders;
using Monolegal.Application.Services;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests.Services;

public sealed class ReminderProcessingServiceTests
{
    [Fact]
    public async Task ProcessAsync_SendsEmailsAndUpdatesOnlyEligibleInvoices()
    {
        var bundle = CreateSut();
        var client = TestData.Client();
        var firstReminder = TestData.Invoice(InvoiceStatus.PrimerRecordatorio, "invoice-1", number: "FV-001");
        var secondReminder = TestData.Invoice(InvoiceStatus.SegundoRecordatorio, "invoice-2", number: "FV-002");
        var paid = TestData.Invoice(InvoiceStatus.Pagada, "invoice-3", number: "FV-003");

        bundle.Clients
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });
        bundle.Invoices
            .Setup(repository => repository.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { firstReminder, secondReminder, paid });
        bundle.Invoices
            .Setup(repository => repository.UpdateStatusAsync(
                It.IsAny<string>(),
                It.IsAny<InvoiceStatus>(),
                It.IsAny<InvoiceStatus>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await bundle.Sut.ProcessAsync();

        Assert.Equal(1, result.ClientsProcessed);
        Assert.Equal(3, result.InvoicesEvaluated);
        Assert.Equal(1, result.InvoicesSkipped);
        Assert.Equal(2, result.EligibleInvoices);
        Assert.Equal(2, result.EmailsSent);
        Assert.Equal(2, result.InvoicesUpdated);
        Assert.Equal(0, result.Failures);
        Assert.Equal(2, result.Actions.Count);

        bundle.Invoices.Verify(repository => repository.UpdateStatusAsync(
            "invoice-1",
            InvoiceStatus.PrimerRecordatorio,
            InvoiceStatus.SegundoRecordatorio,
            It.IsAny<CancellationToken>()), Times.Once);
        bundle.Invoices.Verify(repository => repository.UpdateStatusAsync(
            "invoice-2",
            InvoiceStatus.SegundoRecordatorio,
            InvoiceStatus.Desactivado,
            It.IsAny<CancellationToken>()), Times.Once);
        bundle.Invoices.Verify(repository => repository.UpdateStatusAsync(
            "invoice-3",
            It.IsAny<InvoiceStatus>(),
            It.IsAny<InvoiceStatus>(),
            It.IsAny<CancellationToken>()), Times.Never);

        bundle.Email.Verify(sender => sender.SendAsync(
            It.Is<EmailMessage>(message =>
                message.ToAddress == client.Email &&
                message.Subject.Contains("Segundo recordatorio")),
            It.IsAny<CancellationToken>()), Times.Once);
        bundle.Email.Verify(sender => sender.SendAsync(
            It.Is<EmailMessage>(message => message.Subject.Contains("desactivación")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenEmailFails_DoesNotUpdateInvoiceStatus()
    {
        var bundle = CreateSut();
        var client = TestData.Client();
        var invoice = TestData.Invoice(InvoiceStatus.PrimerRecordatorio);

        bundle.Clients
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });
        bundle.Invoices
            .Setup(repository => repository.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { invoice });
        bundle.Email
            .Setup(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP no disponible"));

        var result = await bundle.Sut.ProcessAsync();

        Assert.Equal(0, result.EmailsSent);
        Assert.Equal(0, result.InvoicesUpdated);
        Assert.Equal(1, result.Failures);
        Assert.False(result.Actions.Single().Succeeded);
        Assert.Contains("SMTP", result.Actions.Single().ErrorMessage);

        bundle.Invoices.Verify(repository => repository.UpdateStatusAsync(
            It.IsAny<string>(),
            It.IsAny<InvoiceStatus>(),
            It.IsAny<InvoiceStatus>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenStatusChangedConcurrently_ReportsFailure()
    {
        var bundle = CreateSut();
        var client = TestData.Client();
        var invoice = TestData.Invoice(InvoiceStatus.PrimerRecordatorio);

        bundle.Clients
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });
        bundle.Invoices
            .Setup(repository => repository.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { invoice });
        bundle.Invoices
            .Setup(repository => repository.UpdateStatusAsync(
                invoice.Id,
                invoice.Status,
                InvoiceStatus.SegundoRecordatorio,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await bundle.Sut.ProcessAsync();

        Assert.Equal(1, result.EmailsSent);
        Assert.Equal(0, result.InvoicesUpdated);
        Assert.Equal(1, result.Failures);
        Assert.Contains("cambió de estado", result.Actions.Single().ErrorMessage);
    }

    [Fact]
    public async Task ProcessAsync_ProcessesThreeClientsEndToEnd()
    {
        var bundle = CreateSut();
        var clients = new[]
        {
            TestData.Client("client-1", "Cliente Uno", "uno@example.com"),
            TestData.Client("client-2", "Cliente Dos", "dos@example.com"),
            TestData.Client("client-3", "Cliente Tres", "tres@example.com")
        };

        bundle.Clients
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        foreach (var client in clients)
        {
            var invoice = TestData.Invoice(
                InvoiceStatus.PrimerRecordatorio,
                $"invoice-{client.Id}",
                client.Id,
                $"FV-{client.Id}");

            bundle.Invoices
                .Setup(repository => repository.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { invoice });
        }

        bundle.Invoices
            .Setup(repository => repository.UpdateStatusAsync(
                It.IsAny<string>(),
                It.IsAny<InvoiceStatus>(),
                It.IsAny<InvoiceStatus>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await bundle.Sut.ProcessAsync();

        Assert.Equal(3, result.ClientsProcessed);
        Assert.Equal(3, result.EligibleInvoices);
        Assert.Equal(3, result.EmailsSent);
        Assert.Equal(3, result.InvoicesUpdated);
        Assert.Equal(0, result.Failures);
        Assert.Equal(3, result.Actions.Select(action => action.ClientId).Distinct().Count());
    }

    [Fact]
    public async Task ProcessAsync_WithSelectedClients_UsesByIdsQuery()
    {
        var bundle = CreateSut();
        var client = TestData.Client();
        var invoice = TestData.Invoice(InvoiceStatus.SegundoRecordatorio);

        bundle.Clients
            .Setup(repository => repository.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });
        bundle.Invoices
            .Setup(repository => repository.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { invoice });
        bundle.Invoices
            .Setup(repository => repository.UpdateStatusAsync(
                It.IsAny<string>(),
                It.IsAny<InvoiceStatus>(),
                It.IsAny<InvoiceStatus>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await bundle.Sut.ProcessAsync(new[] { client.Id, client.Id });

        Assert.Equal(1, result.ClientsProcessed);
        Assert.Equal(1, result.EmailsSent);
        bundle.Clients.Verify(
            repository => repository.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        bundle.Clients.Verify(
            repository => repository.GetByIdsAsync(
                It.Is<IReadOnlyCollection<string>>(ids => ids.Count == 1 && ids.Contains(client.Id)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithNoClients_ReturnsEmptyResult()
    {
        var bundle = CreateSut();
        bundle.Clients
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Client>());

        var result = await bundle.Sut.ProcessAsync();

        Assert.Equal(0, result.ClientsProcessed);
        Assert.Equal(0, result.InvoicesEvaluated);
        Assert.Equal(0, result.EmailsSent);
        Assert.Empty(result.Actions);
        bundle.Email.Verify(
            sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static SutBundle CreateSut()
    {
        var clients = new Mock<IClientRepository>();
        var invoices = new Mock<IInvoiceRepository>();
        var email = new Mock<IEmailSender>();
        var archive = new Mock<IEmailArchive>();
        var resolver = new ReminderTransitionResolver(
            new IReminderTransitionStrategy[]
            {
                new FirstReminderTransitionStrategy(),
                new SecondReminderTransitionStrategy()
            });

        var sut = new ReminderProcessingService(
            clients.Object,
            invoices.Object,
            resolver,
            email.Object,
            archive.Object,
            NullLogger<ReminderProcessingService>.Instance);

        return new SutBundle(sut, clients, invoices, email, archive);
    }

    private sealed record SutBundle(
        ReminderProcessingService Sut,
        Mock<IClientRepository> Clients,
        Mock<IInvoiceRepository> Invoices,
        Mock<IEmailSender> Email,
        Mock<IEmailArchive> Archive);
}
