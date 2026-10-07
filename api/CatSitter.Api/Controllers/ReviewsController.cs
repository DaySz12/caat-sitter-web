using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize(Roles = "owner")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReviewsController(AppDbContext db)
    {
        _db = db;
    }

    /// One review per completed booking. Also refreshes the sitter's RatingAvg / ReviewCount,
    /// which feed the rating (40%) and review-volume (15%) parts of Smart Matching.
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest req)
    {
        if (req.Rating is < 1 or > 5)
            return BadRequest(new { message = "กรุณาให้คะแนน 1-5 ดาว" });

        var ownerId = User.GetUserId();
        var booking = await _db.Bookings.FindAsync(req.BookingId);
        if (booking is null) return NotFound(new { message = "ไม่พบการจองนี้" });
        if (booking.OwnerId != ownerId) return Forbid();
        if (booking.Status != "completed")
            return BadRequest(new { message = "รีวิวได้เมื่องานดูแลเสร็จสิ้นแล้วเท่านั้น" });
        if (await _db.Reviews.AnyAsync(r => r.BookingId == booking.Id))
            return Conflict(new { message = "คุณรีวิวการจองนี้ไปแล้ว" });

        var review = new Review
        {
            BookingId = booking.Id,
            ReviewerId = ownerId,
            SitterId = booking.SitterId,
            Rating = req.Rating,
            Comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim(),
            Photos = req.Photos ?? new List<string>(),
        };
        _db.Reviews.Add(review);

        var profile = await _db.SitterProfiles.FindAsync(booking.SitterId);
        if (profile is not null)
        {
            var ratings = await _db.Reviews.Where(r => r.SitterId == booking.SitterId).Select(r => r.Rating).ToListAsync();
            ratings.Add(req.Rating);
            profile.RatingAvg = Math.Round((decimal)ratings.Average(), 2);
            profile.ReviewCount = ratings.Count;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
        }

        _db.Notifications.Add(new Notification
        {
            UserId = booking.SitterId,
            Title = $"⭐ คุณได้รับรีวิว {req.Rating} ดาว",
            Body = review.Comment ?? $"เจ้าของแมวให้คะแนนงานดูแล {booking.BookingCode}",
            Type = "review_received",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _db.SaveChangesAsync();

        review.Reviewer = await _db.Users.FindAsync(ownerId);
        return Ok(review.ToDto());
    }
}
