namespace Monolegal.Application.Abstractions.Seed;

public interface IDemoDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);

    Task ResetAsync(CancellationToken cancellationToken = default);
}
