using Monolegal.Application.Reminders;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests.Reminders;

public sealed class ReminderTransitionResolverTests
{
    private readonly ReminderTransitionResolver _resolver = new(
        new IReminderTransitionStrategy[]
        {
            new FirstReminderTransitionStrategy(),
            new SecondReminderTransitionStrategy()
        });

    [Theory]
    [InlineData(InvoiceStatus.PrimerRecordatorio, InvoiceStatus.SegundoRecordatorio)]
    [InlineData(InvoiceStatus.SegundoRecordatorio, InvoiceStatus.Desactivado)]
    public void Resolve_ReturnsExpectedTransition(InvoiceStatus current, InvoiceStatus expected)
    {
        var transition = _resolver.Resolve(
            TestData.Client(),
            TestData.Invoice(current));

        Assert.Equal(expected, transition.ToStatus);
    }

    [Fact]
    public void Resolve_ThrowsWhenStatusHasNoStrategy()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _resolver.Resolve(TestData.Client(), TestData.Invoice(InvoiceStatus.AlDia)));

        Assert.Contains("aldia", exception.Message);
    }
}
