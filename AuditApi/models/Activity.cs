using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuditApi.models
{
    public class Activity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public Guid TaskId { get; set; }
        public string Action { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string? PerformedBy { get; set; }
        // a no sql details to add what ever iwant from diffrent services
        public BsonDocument? Details { get; set; }

    }
}