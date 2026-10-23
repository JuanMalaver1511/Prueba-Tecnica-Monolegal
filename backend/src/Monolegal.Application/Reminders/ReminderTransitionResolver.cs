using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

public sealed class ReminderTransitionResolver : IReminderTransitionResolver
{
    private readonly IReadOnlyList<IReminderTransitionStrategy> _strategies;

    public ReminderTransitionResolver(IEnumerable<IReminderTransitionStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public ReminderTransition Resolve(Client client, Invoice invoice)
    {
        var strategy = _strategies.FirstOrDefault(candidate => candidate.CanHandle(invoice.Status));

        return strategy is null
            ? throw new InvalidOperationException(
                $"No existe una transición configurada para el estado '{invoice.Status.ToDbValue()}'.")
            : strategy.Create(client, invoice);
    }
}
