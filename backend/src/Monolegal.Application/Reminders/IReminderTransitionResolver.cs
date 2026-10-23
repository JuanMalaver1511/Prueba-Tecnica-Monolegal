using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

public interface IReminderTransitionResolver
{
    ReminderTransition Resolve(Client client, Invoice invoice);
}
