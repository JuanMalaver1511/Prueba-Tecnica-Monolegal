using Monolegal.Application.Reminders;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests.Reminders;

public sealed class FirstReminderTransitionStrategyTests
{
    private readonly FirstReminderTransitionStrategy _strategy = new();

    [Fact]
    public void CanHandle_ReturnsTrueOnlyForFirstReminder()
    {
        Assert.True(_strategy.CanHandle(InvoiceStatus.PrimerRecordatorio));
        Assert.False(_strategy.CanHandle(InvoiceStatus.SegundoRecordatorio));
        Assert.False(_strategy.CanHandle(InvoiceStatus.Desactivado));
        Assert.False(_strategy.CanHandle(InvoiceStatus.AlDia));
        Assert.False(_strategy.CanHandle(InvoiceStatus.Pagada));
    }

    [Fact]
    public void Create_ReturnsTransitionToSecondReminder()
    {
        var client = TestData.Client();
        var invoice = TestData.Invoice(InvoiceStatus.PrimerRecordatorio);

        var transition = _strategy.Create(client, invoice);

        Assert.Equal(InvoiceStatus.PrimerRecordatorio, transition.FromStatus);
        Assert.Equal(InvoiceStatus.SegundoRecordatorio, transition.ToStatus);
        Assert.Contains(invoice.Number, transition.EmailSubject);
        Assert.Contains("Segundo recordatorio", transition.EmailSubject);
    }

    [Fact]
    public void Create_BuildsPersonalizedHtmlAndPlainText()
    {
        var client = TestData.Client(name: "Ana Torres");
        var invoice = TestData.Invoice(InvoiceStatus.PrimerRecordatorio, number: "FV-777");

        var transition = _strategy.Create(client, invoice);

        Assert.Contains("Ana Torres", transition.HtmlBody);
        Assert.Contains("FV-777", transition.HtmlBody);
        Assert.Contains("segundo recordatorio", transition.HtmlBody);
        Assert.Contains("Ana Torres", transition.PlainTextBody);
        Assert.Contains("1,000,000 COP", transition.PlainTextBody);
    }

    [Fact]
    public void Create_HtmlEncodesClientData()
    {
        var client = TestData.Client(name: "<script>alert('xss')</script>");
        var invoice = TestData.Invoice(InvoiceStatus.PrimerRecordatorio);

        var transition = _strategy.Create(client, invoice);

        Assert.DoesNotContain("<script>", transition.HtmlBody);
        Assert.Contains("&lt;script&gt;", transition.HtmlBody);
    }
}
