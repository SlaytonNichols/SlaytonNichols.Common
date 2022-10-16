using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SlaytonNichols.Common.Infrastructure.MongoDb;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    ObjectId Id { get; set; }

    DateTime CreatedAt { get; }
}