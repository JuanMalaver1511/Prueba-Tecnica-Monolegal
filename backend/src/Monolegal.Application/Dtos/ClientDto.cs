namespace Monolegal.Application.Dtos;

public sealed record ClientDto(
    string Id,
    string Name,
    string Email,
    string Identification,
    bool IsActive);
