namespace CatSitter.Api.Entities;

public class ChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChecklistId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public string? PhotoUrl { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Note { get; set; }

    public Checklist? Checklist { get; set; }
}
