using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private static readonly string[] ValidNextStatuses =
        { "confirmed", "in_progress", "completed", "cancelled", "rejected" };

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Booking> BaseQuery() => _db.Bookings
        .Include(b => b.Owner)
        .Include(b => b.Sitter)
        .Include(b => b.BookingCats).ThenInclude(bc => bc.Cat);

    /// Bookings where the signed-in user is either the owner or the sitter.
    [HttpGet("mine")]
    public async Task<ActionResult<List<BookingDto>>> GetMine([FromQuery] string? status)
    {
        var userId = User.GetUserId();
        var query = BaseQuery().Where(b => b.OwnerId == userId || b.SitterId == userId);

        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(b => b.Status == status);
        }

        var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return Ok(bookings.Select(b => b.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var booking = await BaseQuery().FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null) return NotFound();
        if (booking.OwnerId != userId && booking.SitterId != userId && User.GetRole() != "admin") return Forbid();
        return Ok(booking.ToDto());
    }

    [Authorize(Roles = "owner")]
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest req)
    {
        var ownerId = User.GetUserId();

        if (req.CatIds.Count == 0)
            return BadRequest(new { message = "กรุณาเลือกแมวอย่างน้อย 1 ตัว" });
        if (req.EndDate < req.StartDate)
            return BadRequest(new { message = "วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มต้น" });

        var sitterProfile = await _db.SitterProfiles.FindAsync(req.SitterId);
        if (sitterProfile is null)
            return NotFound(new { message = "ไม่พบผู้รับฝากเลี้ยงรายนี้" });

        var cats = await _db.Cats.Where(c => req.CatIds.Contains(c.Id) && c.OwnerId == ownerId).ToListAsync();
        if (cats.Count != req.CatIds.Count)
            return BadRequest(new { message = "พบข้อมูลแมวที่ไม่ถูกต้อง" });

        var daysCount = req.EndDate.DayNumber - req.StartDate.DayNumber + 1;
        var basePrice = sitterProfile.PricePerDay * daysCount * cats.Count;
        var serviceFee = Math.Round(basePrice * 0.10m, 2);
        var totalPrice = basePrice + serviceFee;

        var booking = new Booking
        {
            BookingCode = $"CS-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
            OwnerId = ownerId,
            SitterId = req.SitterId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            DaysCount = daysCount,
            BasePrice = basePrice,
            ServiceFee = serviceFee,
            TotalPrice = totalPrice,
            SpecialInstructions = req.SpecialInstructions,
            Status = "pending",
            PaymentStatus = "unpaid",
        };
        booking.BookingCats = cats.Select(c => new BookingCat { BookingId = booking.Id, CatId = c.Id }).ToList();

        _db.Bookings.Add(booking);

        _db.Notifications.Add(new Notification
        {
            UserId = req.SitterId,
            Title = "🐾 มีคำขอจองบริการดูแลแมวใหม่!",
            Body = "คุณได้รับคำขอจองบริการใหม่ โปรดตรวจสอบและตอบรับงาน",
            Type = "booking_created",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _db.SaveChangesAsync();

        var created = await BaseQuery().FirstAsync(b => b.Id == booking.Id);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, created.ToDto());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingDto>> UpdateStatus(Guid id, UpdateBookingStatusRequest req)
    {
        if (!ValidNextStatuses.Contains(req.Status))
            return BadRequest(new { message = "สถานะไม่ถูกต้อง" });

        var userId = User.GetUserId();
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();
        if (booking.OwnerId != userId && booking.SitterId != userId && User.GetRole() != "admin") return Forbid();

        // Only the sitter can accept/reject/start/complete; only the owner can cancel a pending request.
        var isSitterAction = req.Status is "confirmed" or "in_progress" or "completed" or "rejected";
        if (isSitterAction && booking.SitterId != userId && User.GetRole() != "admin") return Forbid();
        if (req.Status == "cancelled" && booking.OwnerId != userId && User.GetRole() != "admin") return Forbid();

        booking.Status = req.Status;
        if (req.CancellationReason is not null) booking.CancellationReason = req.CancellationReason;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        var notifyUserId = userId == booking.OwnerId ? booking.SitterId : booking.OwnerId;
        _db.Notifications.Add(new Notification
        {
            UserId = notifyUserId,
            Title = "อัปเดตสถานะการจอง",
            Body = $"การจอง {booking.BookingCode} เปลี่ยนสถานะเป็น {req.Status}",
            Type = "booking_status_changed",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\",\"status\":\"{req.Status}\"}}",
        });

        await _db.SaveChangesAsync();
        var updated = await BaseQuery().FirstAsync(b => b.Id == id);
        return Ok(updated.ToDto());
    }
}
