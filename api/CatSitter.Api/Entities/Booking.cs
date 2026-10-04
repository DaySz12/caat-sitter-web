namespace CatSitter.Api.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BookingCode { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid SitterId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int DaysCount { get; set; } = 1;
    public decimal BasePrice { get; set; } = 0m;
    public decimal ServiceFee { get; set; } = 0m;
    public decimal TotalPrice { get; set; } = 0m;
    public string Status { get; set; } = "pending"; // pending|confirmed|in_progress|completed|cancelled|rejected
    public string PaymentStatus { get; set; } = "unpaid"; // unpaid|paid|refunded
    public string? PaymentIntentId { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? Owner { get; set; }
    public User? Sitter { get; set; }
    public ICollection<BookingCat> BookingCats { get; set; } = new List<BookingCat>();
}
