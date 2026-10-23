using Monolegal.Application.Dtos;

namespace Monolegal.Application.Services;

public interface IReminderProcessingService
{
    Task<ReminderProcessingResult> ProcessAsync(
        IReadOnlyCollection<string>? clientIds = null,
        CancellationToken cancellationToken = default);
}
