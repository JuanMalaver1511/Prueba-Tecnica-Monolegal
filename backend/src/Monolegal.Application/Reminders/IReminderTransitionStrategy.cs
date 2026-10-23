using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

public interface IReminderTransitionStrategy
{
    bool CanHandle(InvoiceStatus currentStatus);

    ReminderTransition Create(Client client, Invoice invoice);
}
