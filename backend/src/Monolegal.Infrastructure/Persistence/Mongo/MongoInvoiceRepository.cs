using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Domain.Entities;
using MongoDB.Driver;
using InvoiceDocument = Monolegal.Infrastructure.Persistence.Mongo.Documents.InvoiceDocument;

namespace Monolegal.Infrastructure.Persistence.Mongo;

public sealed class MongoInvoiceRepository : IInvoiceRepository
{
    private readonly IMongoCollection<InvoiceDocument> _collection;

    public MongoInvoiceRepository(IMongoDatabase database, IOptions<MongoSettings> settings)
    {
        _collection = database.GetCollection<InvoiceDocument>(settings.Value.InvoicesCollectionName);
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(FilterDefinition<InvoiceDocument>.Empty)
            .SortBy(invoice => invoice.DueDate)
            .ThenBy(invoice => invoice.Number)
            .ToListAsync(cancellationToken);

        return documents.Select(Map).ToList();
    }

    public async Task<Invoice?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var document = await _collection
            .Find(invoice => invoice.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : Map(document);
    }

    public async Task<IReadOnlyList<Invoice>> GetByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(invoice => invoice.ClientId == clientId)
            .SortBy(invoice => invoice.DueDate)
            .ThenBy(invoice => invoice.Number)
            .ToListAsync(cancellationToken);

        return documents.Select(Map).ToList();
    }

    public async Task<bool> UpdateStatusAsync(
        string invoiceId,
        InvoiceStatus expectedStatus,
        InvoiceStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<InvoiceDocument>.Filter.And(
            Builders<InvoiceDocument>.Filter.Eq(invoice => invoice.Id, invoiceId),
            Builders<InvoiceDocument>.Filter.Eq(invoice => invoice.Status, expectedStatus.ToDbValue()));

        var update = Builders<InvoiceDocument>.Update.Set(invoice => invoice.Status, newStatus.ToDbValue());
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        return result.MatchedCount == 1;
    }

    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var document = new InvoiceDocument
        {
            Id = invoice.Id,
            ClientId = invoice.ClientId,
            Number = invoice.Number,
            Description = invoice.Description,
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToDbValue()
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return invoice;
    }

    public async Task<bool> InvoiceNumberExistsAsync(string number, CancellationToken cancellationToken = default)
    {
        var count = await _collection.CountDocumentsAsync(invoice => invoice.Number == number, cancellationToken: cancellationToken);
        return count > 0;
    }

    private static Invoice Map(InvoiceDocument document) =>
        new()
        {
            Id = document.Id,
            ClientId = document.ClientId,
            Number = document.Number,
            Description = document.Description,
            Amount = document.Amount,
            Currency = document.Currency,
            IssueDate = document.IssueDate,
            DueDate = document.DueDate,
            Status = InvoiceStatusExtensions.FromDbValue(document.Status)
        };
}
