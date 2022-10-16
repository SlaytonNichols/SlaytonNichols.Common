using MongoDB.Bson;

namespace SlaytonNichols.Common.Infrastructure.MongoDb;

public abstract class Document : IDocument
{
    public ObjectId Id { get; set; }

    public DateTime CreatedAt => Id.CreationTime;
}