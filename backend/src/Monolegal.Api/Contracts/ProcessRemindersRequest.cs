namespace Monolegal.Api.Contracts;

public sealed record ProcessRemindersRequest(IReadOnlyCollection<string>? ClientIds);
