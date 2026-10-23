using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Domain.Entities;
using MongoDB.Driver;
using ClientEntity = Monolegal.Domain.Entities.Client;
using ClientDocument = Monolegal.Infrastructure.Persistence.Mongo.Documents.ClientDocument;

namespace Monolegal.Infrastructure.Persistence.Mongo;

public sealed class MongoClientRepository : IClientRepository
{
    private readonly IMongoCollection<ClientDocument> _collection;

    public MongoClientRepository(IMongoDatabase database, IOptions<MongoSettings> settings)
    {
        _collection = database.GetCollection<ClientDocument>(settings.Value.ClientsCollectionName);
    }

    public async Task<IReadOnlyList<ClientEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(FilterDefinition<ClientDocument>.Empty)
            .SortBy(client => client.Name)
            .ToListAsync(cancellationToken);

        return documents.Select(Map).ToList();
    }

    public async Task<ClientEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _collection
            .Find(client => client.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : Map(document);
    }

    public async Task<IReadOnlyList<ClientEntity>> GetByIdsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<ClientEntity>();
        }

        var filter = Builders<ClientDocument>.Filter.In(client => client.Id, ids);
        var documents = await _collection.Find(filter).SortBy(client => client.Name).ToListAsync(cancellationToken);

        return documents.Select(Map).ToList();
    }

    public async Task<ClientEntity> CreateAsync(ClientEntity client, CancellationToken cancellationToken = default)
    {
        var document = new ClientDocument
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Identification = client.Identification,
            IsActive = client.IsActive
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return client;
    }

    private static ClientEntity Map(ClientDocument document) =>
        new()
        {
            Id = document.Id,
            Name = document.Name,
            Email = document.Email,
            Identification = document.Identification,
            IsActive = document.IsActive
        };
}
