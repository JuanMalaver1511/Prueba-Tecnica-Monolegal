using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monolegal.Infrastructure.Persistence.Mongo.Documents;

public sealed class ClientDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("identification")]
    public string Identification { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; }
}
