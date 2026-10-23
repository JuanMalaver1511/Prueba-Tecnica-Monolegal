namespace Monolegal.Infrastructure.Persistence.Mongo;

public sealed class MongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    public string DatabaseName { get; set; } = "MonolegalDb";

    public string ClientsCollectionName { get; set; } = "clients";

    public string InvoicesCollectionName { get; set; } = "invoices";
}
