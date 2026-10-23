using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests.Domain;

public sealed class InvoiceStatusExtensionsTests
{
    [Theory]
    [InlineData(InvoiceStatus.PrimerRecordatorio, "primerrecordatorio")]
    [InlineData(InvoiceStatus.SegundoRecordatorio, "segundorecordatorio")]
    [InlineData(InvoiceStatus.Desactivado, "desactivado")]
    [InlineData(InvoiceStatus.AlDia, "aldia")]
    [InlineData(InvoiceStatus.Pagada, "pagada")]
    public void DatabaseMapping_RoundTrips(InvoiceStatus status, string databaseValue)
    {
        Assert.Equal(databaseValue, status.ToDbValue());
        Assert.Equal(status, InvoiceStatusExtensions.FromDbValue(databaseValue));
        Assert.Equal(status, InvoiceStatusExtensions.FromDbValue(databaseValue.ToUpperInvariant()));
    }

    [Theory]
    [InlineData(InvoiceStatus.PrimerRecordatorio, true)]
    [InlineData(InvoiceStatus.SegundoRecordatorio, true)]
    [InlineData(InvoiceStatus.Desactivado, false)]
    [InlineData(InvoiceStatus.AlDia, false)]
    [InlineData(InvoiceStatus.Pagada, false)]
    public void IsReminderPending_ReturnsExpectedValue(InvoiceStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsReminderPending());
    }
}
