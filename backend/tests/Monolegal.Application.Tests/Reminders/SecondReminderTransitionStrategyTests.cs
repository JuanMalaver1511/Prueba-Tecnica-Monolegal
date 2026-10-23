using Monolegal.Application.Reminders;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests.Reminders;

public sealed class SecondReminderTransitionStrategyTests
{
    private readonly SecondReminderTransitionStrategy _strategy = new();

    [Fact]
    public void CanHandle_ReturnsTrueOnlyForSecondReminder()
    {
        Assert.True(_strategy.CanHandle(InvoiceStatus.SegundoRecordatorio));
        Assert.False(_strategy.CanHandle(InvoiceStatus.PrimerRecordatorio));
        Assert.False(_strategy.CanHandle(InvoiceStatus.Desactivado));
    }

    [Fact]
    public void Create_ReturnsTransitionToDeactivated()
    {
        var client = TestData.Client();
        var invoice = TestData.Invoice(InvoiceStatus.SegundoRecordatorio);

        var transition = _strategy.Create(client, invoice);

        Assert.Equal(InvoiceStatus.SegundoRecordatorio, transition.FromStatus);
        Assert.Equal(InvoiceStatus.Desactivado, transition.ToStatus);
        Assert.Contains("desactivación", transition.EmailSubject);
    }

    [Fact]
    public void Create_MessageExplainsUpcomingDeactivation()
    {
        var client = TestData.Client();
        var invoice = TestData.Invoice(InvoiceStatus.SegundoRecordatorio, number: "FV-888");

        var transition = _strategy.Create(client, invoice);

        Assert.Contains("desactivada", transition.HtmlBody);
        Assert.Contains("FV-888", transition.PlainTextBody);
        Assert.Contains("Segundo recordatorio", transition.PlainTextBody);
    }
}
