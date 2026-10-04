namespace CatSitter.Api.Entities;

public class ChatRoom
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid SitterId { get; set; }
    public Guid? BookingId { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTimeOffset LastMessageTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
