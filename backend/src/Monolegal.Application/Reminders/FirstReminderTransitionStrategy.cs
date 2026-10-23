using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

public sealed class FirstReminderTransitionStrategy : IReminderTransitionStrategy
{
    public bool CanHandle(InvoiceStatus currentStatus) =>
        currentStatus is InvoiceStatus.PrimerRecordatorio;

    public ReminderTransition Create(Client client, Invoice invoice)
    {
        const string heading = "Su factura pasó a segundo recordatorio";
        var message =
            $"Le informamos que la factura {invoice.Number} ha pasado a segundo recordatorio. " +
            "Por favor realice el pago para evitar la desactivación del servicio.";

        return new ReminderTransition(
            InvoiceStatus.PrimerRecordatorio,
            InvoiceStatus.SegundoRecordatorio,
            $"Segundo recordatorio de pago - Factura {invoice.Number}",
            ReminderEmailContent.BuildHtml(client, invoice, heading, message),
            ReminderEmailContent.BuildPlainText(client, invoice, message));
    }
}
