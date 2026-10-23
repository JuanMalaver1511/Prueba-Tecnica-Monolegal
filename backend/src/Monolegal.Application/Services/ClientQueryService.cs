using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Dtos;

namespace Monolegal.Application.Services;

public sealed class ClientQueryService : IClientQueryService
{
    private readonly IClientRepository _clientRepository;

    public ClientQueryService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetAllAsync(cancellationToken);

        return clients
            .OrderBy(client => client.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(ToDto)
            .ToList();
    }

    public async Task<ClientDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);

        return client is null ? null : ToDto(client);
    }

    private static ClientDto ToDto(Domain.Entities.Client client) =>
        new(client.Id, client.Name, client.Email, client.Identification, client.IsActive);
}
