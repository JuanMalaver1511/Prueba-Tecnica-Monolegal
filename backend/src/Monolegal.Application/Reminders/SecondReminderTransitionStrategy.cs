using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

public sealed class SecondReminderTransitionStrategy : IReminderTransitionStrategy
{
    public bool CanHandle(InvoiceStatus currentStatus) =>
        currentStatus is InvoiceStatus.SegundoRecordatorio;

    public ReminderTransition Create(Client client, Invoice invoice)
    {
        const string heading = "Aviso de desactivación de su factura";
        var message =
            $"Le informamos que la factura {invoice.Number} se encuentra en segundo recordatorio " +
            "y será desactivada. Por favor comuníquese con nosotros o realice el pago de inmediato.";

        return new ReminderTransition(
            InvoiceStatus.SegundoRecordatorio,
            InvoiceStatus.Desactivado,
            $"Aviso de desactivación - Factura {invoice.Number}",
            ReminderEmailContent.BuildHtml(client, invoice, heading, message),
            ReminderEmailContent.BuildPlainText(client, invoice, message));
    }
}
