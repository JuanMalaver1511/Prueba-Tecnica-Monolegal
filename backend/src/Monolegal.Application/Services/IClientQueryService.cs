using Monolegal.Application.Dtos;

namespace Monolegal.Application.Services;

public interface IClientQueryService
{
    Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ClientDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
