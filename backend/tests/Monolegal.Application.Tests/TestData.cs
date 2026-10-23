using Monolegal.Domain.Entities;

namespace Monolegal.Application.Tests;

internal static class TestData
{
    public static Client Client(
        string id = "client-1",
        string name = "Ana Torres",
        string email = "ana@example.com") =>
        new()
        {
            Id = id,
            Name = name,
            Email = email,
            Identification = "CC 123",
            IsActive = true
        };

    public static Invoice Invoice(
        InvoiceStatus status,
        string id = "invoice-1",
        string clientId = "client-1",
        string number = "FV-001",
        decimal amount = 1_000_000m) =>
        new()
        {
            Id = id,
            ClientId = clientId,
            Number = number,
            Description = "Servicio de prueba",
            Amount = amount,
            Currency = "COP",
            IssueDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DueDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = status
        };
}
