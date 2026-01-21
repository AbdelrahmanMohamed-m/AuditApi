
using System.Text.Json;
using AuditApi.DTOs;
using AuditApi.models;
using MongoDB.Bson;

namespace AuditApi.mappers
{
    public class ActvityMapper
    {

        public static ActivityDto ToDto(Activity activity)
        {
            return new ActivityDto
            {
                TaskId = activity.TaskId,
                Action = activity.Action,
                Timestamp = activity.Timestamp,
                PerformedBy = activity.PerformedBy,
                Details = activity.Details?.ToDictionary(e => e.Name, e => BsonTypeMapper.MapToDotNetValue(e.Value))
            };
        }

        public static Activity ToModel(ActivityDto dto)
        {
            return new Activity
            {
                TaskId = dto.TaskId,
                Action = dto.Action,
                Timestamp = dto.Timestamp,
                PerformedBy = dto.PerformedBy,
                Details = dto.Details != null ? new BsonDocument(dto.Details.ToDictionary(kv => kv.Key, kv => ConvertToBsonCompatible(kv.Value))) : null
            };
        }

        private static object ConvertToBsonCompatible(object value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.TryGetInt32(out int i) ? i : element.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                    JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(element.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                    _ => element.GetRawText()
                };
            }
            return value;
        }
    }
}
