using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monolegal.Infrastructure.Persistence.Mongo.Documents;

public sealed class InvoiceDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [BsonElement("number")]
    public string Number { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "COP";

    [BsonElement("issueDate")]
    public DateTime IssueDate { get; set; }

    [BsonElement("dueDate")]
    public DateTime DueDate { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;
}
