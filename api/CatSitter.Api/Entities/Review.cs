namespace CatSitter.Api.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid SitterId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> Photos { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? Reviewer { get; set; }
}
