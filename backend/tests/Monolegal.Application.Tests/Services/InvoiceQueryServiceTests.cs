using Moq;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Services;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests.Services;

public sealed class InvoiceQueryServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ComputesCountsTotalsAndPendingReminders()
    {
        var client = TestData.Client();
        var invoices = new[]
        {
            TestData.Invoice(InvoiceStatus.PrimerRecordatorio, "1", amount: 100_000m),
            TestData.Invoice(InvoiceStatus.PrimerRecordatorio, "2", amount: 250_000m),
            TestData.Invoice(InvoiceStatus.SegundoRecordatorio, "3", amount: 500_000m),
            TestData.Invoice(InvoiceStatus.Pagada, "4", amount: 1_000_000m)
        };

        var invoiceRepository = new Mock<IInvoiceRepository>();
        invoiceRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);

        var clientRepository = new Mock<IClientRepository>();
        clientRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });

        var service = new InvoiceQueryService(invoiceRepository.Object, clientRepository.Object);

        var summary = await service.GetSummaryAsync(null);

        Assert.Equal(4, summary.TotalInvoices);
        Assert.Equal(1_850_000m, summary.TotalAmount);
        Assert.Equal("COP", summary.Currency);
        Assert.Equal(3, summary.PendingReminders);
        Assert.Equal(2, summary.ByStatus.Single(status => status.Status == "primerrecordatorio").Count);
        Assert.Equal(1, summary.ByStatus.Single(status => status.Status == "segundorecordatorio").Count);
        Assert.Equal(1_000_000m, summary.ByStatus.Single(status => status.Status == "pagada").TotalAmount);
        Assert.Equal(5, summary.ByStatus.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByClientAndMapsClientName()
    {
        var client = TestData.Client("client-1", "Ana Torres");
        var invoice = TestData.Invoice(InvoiceStatus.PrimerRecordatorio, clientId: client.Id);

        var invoiceRepository = new Mock<IInvoiceRepository>();
        invoiceRepository
            .Setup(repository => repository.GetByClientIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { invoice });

        var clientRepository = new Mock<IClientRepository>();
        clientRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { client });

        var service = new InvoiceQueryService(invoiceRepository.Object, clientRepository.Object);

        var result = await service.GetAllAsync(client.Id);

        var mapped = Assert.Single(result);
        Assert.Equal("Ana Torres", mapped.ClientName);
        Assert.Equal("primerrecordatorio", mapped.Status);
        Assert.Equal("Primer recordatorio", mapped.StatusLabel);
        Assert.True(mapped.IsReminderPending);
        invoiceRepository.Verify(
            repository => repository.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
