namespace Monolegal.Application.Dtos;

public sealed record CreateClientRequest(
    string Name,
    string Email,
    string Identification,
    bool IsActive = true);
