namespace Monolegal.Domain.Entities;

public sealed class Client
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Identification { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}
