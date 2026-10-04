namespace CatSitter.Api.Entities;

public class Checklist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid? CatId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}
