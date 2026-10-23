using Monolegal.Domain.Entities;

namespace Monolegal.Application.Abstractions.Persistence;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Client?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Client>> GetByIdsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken = default);

    Task<Client> CreateAsync(Client client, CancellationToken cancellationToken = default);
}
