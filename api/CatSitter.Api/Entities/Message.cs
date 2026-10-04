namespace CatSitter.Api.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
