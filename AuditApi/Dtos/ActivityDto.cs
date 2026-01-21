using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace AuditApi.DTOs;

public class ActivityDto
{
    [Required]
    public Guid TaskId { get; set; }
    [Required]
    public string Action { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? PerformedBy { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}
