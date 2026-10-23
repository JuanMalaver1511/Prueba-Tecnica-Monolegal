using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Email;
using Monolegal.Application.Abstractions.Seed;
using Monolegal.Domain.Entities;
using MongoDB.Driver;
using ClientDocument = Monolegal.Infrastructure.Persistence.Mongo.Documents.ClientDocument;
using InvoiceDocument = Monolegal.Infrastructure.Persistence.Mongo.Documents.InvoiceDocument;

namespace Monolegal.Infrastructure.Persistence.Mongo;

public sealed class MongoDemoDataSeeder : IDemoDataSeeder
{
    private readonly IMongoCollection<ClientDocument> _clients;
    private readonly IMongoCollection<InvoiceDocument> _invoices;
    private readonly IEmailArchive _emailArchive;
    private readonly ILogger<MongoDemoDataSeeder> _logger;

    public MongoDemoDataSeeder(
        IMongoDatabase database,
        IOptions<MongoSettings> settings,
        IEmailArchive emailArchive,
        ILogger<MongoDemoDataSeeder> logger)
    {
        _clients = database.GetCollection<ClientDocument>(settings.Value.ClientsCollectionName);
        _invoices = database.GetCollection<InvoiceDocument>(settings.Value.InvoicesCollectionName);
        _emailArchive = emailArchive;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);

        var existingClients = await _clients.CountDocumentsAsync(
            FilterDefinition<ClientDocument>.Empty,
            cancellationToken: cancellationToken);

        if (existingClients > 0)
        {
            _logger.LogInformation("La base de datos ya contiene clientes; no se insertan datos demo.");
            return;
        }

        await InsertDemoDataAsync(cancellationToken);
        _logger.LogInformation("Datos demo insertados: 3 clientes y 12 facturas.");
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await _invoices.DeleteManyAsync(FilterDefinition<InvoiceDocument>.Empty, cancellationToken);
        await _clients.DeleteManyAsync(FilterDefinition<ClientDocument>.Empty, cancellationToken);
        await _emailArchive.ClearAsync(cancellationToken);

        await EnsureIndexesAsync(cancellationToken);
        await InsertDemoDataAsync(cancellationToken);

        _logger.LogInformation("Datos demo restaurados: 3 clientes y 12 facturas.");
    }

    private async Task InsertDemoDataAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var clients = new List<ClientDocument>
        {
            new()
            {
                Id = "client-ana-torres",
                Name = "Ana María Torres",
                Email = "ana.torres@example.com",
                Identification = "CC 1032456789",
                IsActive = true
            },
            new()
            {
                Id = "client-carlos-ruiz",
                Name = "Carlos Andrés Ruiz",
                Email = "carlos.ruiz@example.com",
                Identification = "NIT 900123456-7",
                IsActive = true
            },
            new()
            {
                Id = "client-laura-gomez",
                Name = "Laura Gómez",
                Email = "laura.gomez@example.com",
                Identification = "CC 52987654",
                IsActive = true
            }
        };

        var invoices = new List<InvoiceDocument>
        {
            CreateInvoice("invoice-1001", "client-ana-torres", "FV-2026-1001", "Licencia anual software contable", 1_450_000m, today.AddDays(-50), today.AddDays(-20), InvoiceStatus.PrimerRecordatorio),
            CreateInvoice("invoice-1002", "client-ana-torres", "FV-2026-1002", "Asesoría tributaria", 890_000m, today.AddDays(-65), today.AddDays(-35), InvoiceStatus.SegundoRecordatorio),
            CreateInvoice("invoice-1003", "client-ana-torres", "FV-2026-1003", "Implementación módulo de nómina", 2_300_000m, today.AddDays(-15), today.AddDays(5), InvoiceStatus.AlDia),
            CreateInvoice("invoice-1004", "client-ana-torres", "FV-2026-1004", "Soporte técnico mensual", 650_000m, today.AddDays(-90), today.AddDays(-60), InvoiceStatus.Pagada),

            CreateInvoice("invoice-2001", "client-carlos-ruiz", "FV-2026-2001", "Auditoría financiera", 3_200_000m, today.AddDays(-42), today.AddDays(-12), InvoiceStatus.PrimerRecordatorio),
            CreateInvoice("invoice-2002", "client-carlos-ruiz", "FV-2026-2002", "Capacitación equipos", 1_100_000m, today.AddDays(-35), today.AddDays(-5), InvoiceStatus.PrimerRecordatorio),
            CreateInvoice("invoice-2003", "client-carlos-ruiz", "FV-2026-2003", "Mantenimiento plataforma", 780_000m, today.AddDays(-70), today.AddDays(-40), InvoiceStatus.SegundoRecordatorio),
            CreateInvoice("invoice-2004", "client-carlos-ruiz", "FV-2026-2004", "Bolsa de horas", 450_000m, today.AddDays(-120), today.AddDays(-90), InvoiceStatus.Desactivado),

            CreateInvoice("invoice-3001", "client-laura-gomez", "FV-2026-3001", "Desarrollo aplicación móvil", 2_050_000m, today.AddDays(-55), today.AddDays(-25), InvoiceStatus.SegundoRecordatorio),
            CreateInvoice("invoice-3002", "client-laura-gomez", "FV-2026-3002", "Integración pasarela de pagos", 980_000m, today.AddDays(-38), today.AddDays(-8), InvoiceStatus.PrimerRecordatorio),
            CreateInvoice("invoice-3003", "client-laura-gomez", "FV-2026-3003", "Rediseño portal web", 1_750_000m, today.AddDays(-100), today.AddDays(-70), InvoiceStatus.Pagada),
            CreateInvoice("invoice-3004", "client-laura-gomez", "FV-2026-3004", "Monitoreo mensual", 560_000m, today.AddDays(-10), today.AddDays(10), InvoiceStatus.AlDia)
        };

        await _clients.InsertManyAsync(clients, cancellationToken: cancellationToken);
        await _invoices.InsertManyAsync(invoices, cancellationToken: cancellationToken);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        var clientIndex = new CreateIndexModel<ClientDocument>(
            Builders<ClientDocument>.IndexKeys.Ascending(client => client.Email));

        var invoiceNumberIndex = new CreateIndexModel<InvoiceDocument>(
            Builders<InvoiceDocument>.IndexKeys.Ascending(invoice => invoice.Number),
            new CreateIndexOptions { Unique = true });

        var invoiceClientIndex = new CreateIndexModel<InvoiceDocument>(
            Builders<InvoiceDocument>.IndexKeys.Ascending(invoice => invoice.ClientId));

        await _clients.Indexes.CreateOneAsync(clientIndex, cancellationToken: cancellationToken);
        await _invoices.Indexes.CreateOneAsync(invoiceNumberIndex, cancellationToken: cancellationToken);
        await _invoices.Indexes.CreateOneAsync(invoiceClientIndex, cancellationToken: cancellationToken);
    }

    private static InvoiceDocument CreateInvoice(
        string id,
        string clientId,
        string number,
        string description,
        decimal amount,
        DateTime issueDate,
        DateTime dueDate,
        InvoiceStatus status) =>
        new()
        {
            Id = id,
            ClientId = clientId,
            Number = number,
            Description = description,
            Amount = amount,
            Currency = "COP",
            IssueDate = issueDate,
            DueDate = dueDate,
            Status = status.ToDbValue()
        };
}
