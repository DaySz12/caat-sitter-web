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
    private static readonly Dictionary<string, string> StatusChangeTexts = new()
    {
        ["confirmed"] = "ได้รับการยืนยันแล้ว",
        ["in_progress"] = "เริ่มการดูแลแล้ว",
        ["completed"] = "เสร็จสิ้นแล้ว",
        ["cancelled"] = "ถูกยกเลิก",
        ["rejected"] = "ถูกปฏิเสธ",
    };

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Booking> BaseQuery() => _db.Bookings
        .Include(b => b.Owner)
        .Include(b => b.Sitter)
        .Include(b => b.BookingCats).ThenInclude(bc => bc.Cat);

    private async Task<List<BookingDto>> ToDtosWithReviews(List<Booking> bookings)
    {
        var ids = bookings.Select(b => b.Id).ToList();
        var reviews = await _db.Reviews
            .Include(r => r.Reviewer)
            .Where(r => ids.Contains(r.BookingId))
            .ToDictionaryAsync(r => r.BookingId);
        return bookings.Select(b => b.ToDto(reviews.GetValueOrDefault(b.Id))).ToList();
    }

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
        return Ok(await ToDtosWithReviews(bookings));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var booking = await BaseQuery().FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null) return NotFound();
        if (booking.OwnerId != userId && booking.SitterId != userId && User.GetRole() != "admin") return Forbid();
        return Ok((await ToDtosWithReviews(new List<Booking> { booking }))[0]);
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

        // Finishing the job closes a visit the sitter forgot to check out of.
        if (req.Status == "completed")
        {
            var openVisits = await _db.AttendanceRecords.Where(a => a.BookingId == id && a.CheckOutTime == null).ToListAsync();
            foreach (var visit in openVisits) visit.CheckOutTime = DateTimeOffset.UtcNow;

            // Credit the sitter's wallet with earnings from completed booking
            var sitterUser = await _db.Users.FindAsync(booking.SitterId);
            if (sitterUser is not null)
            {
                sitterUser.WalletBalance += booking.BasePrice;
            }
        }

        var notifyUserId = userId == booking.OwnerId ? booking.SitterId : booking.OwnerId;
        var body = req.Status == "completed" && notifyUserId == booking.OwnerId
            ? $"การจอง {booking.BookingCode} เสร็จสิ้นแล้ว อย่าลืมให้คะแนนพี่เลี้ยงนะ ⭐"
            : $"การจอง {booking.BookingCode} {StatusChangeTexts[req.Status]}";
        _db.Notifications.Add(new Notification
        {
            UserId = notifyUserId,
            Title = "อัปเดตสถานะการจอง",
            Body = body,
            Type = "booking_status_changed",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\",\"status\":\"{req.Status}\"}}",
        });

        await _db.SaveChangesAsync();
        var updated = await BaseQuery().FirstAsync(b => b.Id == id);
        return Ok(updated.ToDto());
    }

    [HttpPost("{id:guid}/payment-slip")]
    public async Task<ActionResult<BookingDto>> UploadPaymentSlip(Guid id, UploadPaymentSlipRequest req)
    {
        var userId = User.GetUserId();
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();
        if (booking.OwnerId != userId && User.GetRole() != "admin") return Forbid();

        if (string.IsNullOrWhiteSpace(req.SlipUrl))
            return BadRequest(new { message = "กรุณาระบุ URL สลิปการโอนเงิน" });

        booking.PaymentSlipUrl = req.SlipUrl.Trim();
        booking.PaymentSlipUploadedAt = DateTimeOffset.UtcNow;
        booking.PaymentStatus = "pending_verification";
        booking.PaymentRejectionReason = null;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        // Notify admins about the new payment slip
        var adminIds = await _db.Users.Where(u => u.Role == "admin").Select(u => u.Id).ToListAsync();
        foreach (var adminId in adminIds)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = adminId,
                Title = "🧾 มีการแนบสลิปชำระเงินใหม่",
                Body = $"การจอง {booking.BookingCode} มียอดชำระ {booking.TotalPrice:N2} บาท รอการตรวจสอบสลิป",
                Type = "payment_slip_uploaded",
                DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
            });
        }

        await _db.SaveChangesAsync();
        var updated = await BaseQuery().FirstAsync(b => b.Id == id);
        return Ok(updated.ToDto());
    }
}
