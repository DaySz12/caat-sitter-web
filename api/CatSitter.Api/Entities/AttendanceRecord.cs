namespace CatSitter.Api.Entities;

public class AttendanceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid SitterId { get; set; }
    public DateTimeOffset CheckInTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CheckOutTime { get; set; }
    public double? LocationLat { get; set; }
    public double? LocationLng { get; set; }
    public string? CheckinPhotoUrl { get; set; }
    public string? CheckoutPhotoUrl { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
