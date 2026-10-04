namespace CatSitter.Api.Entities;

public class BookingCat
{
    public Guid BookingId { get; set; }
    public Guid CatId { get; set; }

    public Booking? Booking { get; set; }
    public Cat? Cat { get; set; }
}
