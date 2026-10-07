using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

/// The daily care log of a booking: sitter check-in/check-out visits and the per-day checklist.
/// Owner and sitter can both read it; only the booking's sitter can write to it.
/// Every write returns the whole log so the page can re-render in one go, and pushes a live
/// "care" event so an owner watching the booking sees it update.
[ApiController]
[Route("api")]
[Authorize]
public class CareController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LiveEvents _events;

    public CareController(AppDbContext db, LiveEvents events)
    {
        _db = db;
        _events = events;
    }

    [HttpGet("bookings/{id:guid}/care")]
    public async Task<ActionResult<BookingCareDto>> GetCare(Guid id)
    {
        var userId = User.GetUserId();
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();
        if (booking.OwnerId != userId && booking.SitterId != userId && User.GetRole() != "admin") return Forbid();

        return Ok(await LoadCare(id));
    }

    /// Starts a visit. Checking in on a confirmed booking also moves it to in_progress,
    /// and the first check-in of a day creates that day's checklist.
    [HttpPost("bookings/{id:guid}/check-in")]
    public async Task<ActionResult<BookingCareDto>> CheckIn(Guid id, CheckInRequest req)
    {
        var userId = User.GetUserId();
        var booking = await _db.Bookings
            .Include(b => b.BookingCats).ThenInclude(bc => bc.Cat)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null) return NotFound();
        if (booking.SitterId != userId) return Forbid();
        if (booking.Status is not ("confirmed" or "in_progress"))
            return BadRequest(new { message = "เช็กอินได้เฉพาะงานที่ยืนยันแล้วหรือกำลังดูแลอยู่" });

        var today = BangkokTime.Today;
        if (today < booking.StartDate || today > booking.EndDate)
            return BadRequest(new { message = "วันนี้ไม่อยู่ในช่วงวันที่จอง" });
        if (await _db.AttendanceRecords.AnyAsync(a => a.BookingId == id && a.CheckOutTime == null))
            return BadRequest(new { message = "คุณเช็กอินอยู่แล้ว กรุณาเช็กเอาต์ก่อน" });

        var now = DateTimeOffset.UtcNow;
        if (booking.Status == "confirmed")
        {
            booking.Status = "in_progress";
            booking.UpdatedAt = now;
        }

        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            BookingId = id,
            SitterId = userId,
            CheckInTime = now,
            LocationLat = req.Lat,
            LocationLng = req.Lng,
            CheckinPhotoUrl = req.PhotoUrl,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
        });

        if (!await _db.Checklists.AnyAsync(c => c.BookingId == id && c.Date == today))
        {
            var hasHealthNotes = booking.BookingCats.Any(bc => !string.IsNullOrWhiteSpace(bc.Cat?.MedicalNotes));
            _db.Checklists.Add(DailyChecklist.Create(id, today, hasHealthNotes));
        }

        _db.Notifications.Add(new Notification
        {
            UserId = booking.OwnerId,
            Title = "📍 พี่เลี้ยงเช็กอินแล้ว",
            Body = $"พี่เลี้ยงมาถึงและเริ่มดูแลน้องแมวแล้ว (การจอง {booking.BookingCode})",
            Type = "care_check_in",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _db.SaveChangesAsync();
        return Ok(await LoadCareAndPublish(booking));
    }

    [HttpPost("bookings/{id:guid}/check-out")]
    public async Task<ActionResult<BookingCareDto>> CheckOut(Guid id, CheckOutRequest req)
    {
        var userId = User.GetUserId();
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();
        if (booking.SitterId != userId) return Forbid();

        var open = await _db.AttendanceRecords.FirstOrDefaultAsync(a => a.BookingId == id && a.CheckOutTime == null);
        if (open is null) return BadRequest(new { message = "ยังไม่ได้เช็กอิน" });

        open.CheckOutTime = DateTimeOffset.UtcNow;
        open.CheckoutPhotoUrl = req.PhotoUrl;
        if (!string.IsNullOrWhiteSpace(req.Note)) open.Note = req.Note.Trim();

        _db.Notifications.Add(new Notification
        {
            UserId = booking.OwnerId,
            Title = "🏠 พี่เลี้ยงเช็กเอาต์แล้ว",
            Body = $"ดูสรุปงานดูแลรอบนี้ได้ในหน้ารายละเอียดการจอง {booking.BookingCode}",
            Type = "care_check_out",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _db.SaveChangesAsync();
        return Ok(await LoadCareAndPublish(booking));
    }

    [HttpPatch("checklist-items/{itemId:guid}")]
    public async Task<ActionResult<BookingCareDto>> UpdateChecklistItem(Guid itemId, UpdateChecklistItemRequest req)
    {
        var userId = User.GetUserId();
        var item = await _db.ChecklistItems.Include(i => i.Checklist).FirstOrDefaultAsync(i => i.Id == itemId);
        if (item?.Checklist is null) return NotFound();

        var booking = await _db.Bookings.FindAsync(item.Checklist.BookingId);
        if (booking is null) return NotFound();
        if (booking.SitterId != userId) return Forbid();
        if (booking.Status != "in_progress")
            return BadRequest(new { message = "อัปเดตเช็กลิสต์ได้เฉพาะงานที่กำลังดูแลอยู่" });

        var wasCompleted = item.IsCompleted;
        if (req.IsCompleted is { } completed)
        {
            item.IsCompleted = completed;
            item.CompletedAt = completed ? DateTimeOffset.UtcNow : null;
        }
        if (req.PhotoUrl is not null) item.PhotoUrl = req.PhotoUrl == string.Empty ? null : req.PhotoUrl;
        if (req.Note is not null) item.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();

        // Tell the owner once the day's last task gets ticked.
        if (!wasCompleted && item.IsCompleted &&
            await _db.ChecklistItems.Where(i => i.ChecklistId == item.ChecklistId && i.Id != item.Id).AllAsync(i => i.IsCompleted))
        {
            _db.Notifications.Add(new Notification
            {
                UserId = booking.OwnerId,
                Title = "✅ งานดูแลวันนี้ครบทุกข้อแล้ว",
                Body = $"พี่เลี้ยงทำเช็กลิสต์ประจำวันครบแล้ว (การจอง {booking.BookingCode})",
                Type = "care_checklist_done",
                DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
            });
        }

        await _db.SaveChangesAsync();
        return Ok(await LoadCareAndPublish(booking));
    }

    private async Task<BookingCareDto> LoadCareAndPublish(Booking booking)
    {
        var care = await LoadCare(booking.Id);
        _events.Publish(booking.OwnerId, "care", new { bookingId = booking.Id });
        _events.Publish(booking.SitterId, "care", new { bookingId = booking.Id });
        return care;
    }

    private async Task<BookingCareDto> LoadCare(Guid bookingId)
    {
        var attendance = await _db.AttendanceRecords
            .Where(a => a.BookingId == bookingId)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();

        var checklists = await _db.Checklists
            .Include(c => c.Items.OrderBy(i => i.Id))
            .Where(c => c.BookingId == bookingId)
            .OrderByDescending(c => c.Date)
            .ToListAsync();

        return new BookingCareDto(
            attendance.Select(a => a.ToDto()).ToList(),
            checklists.Select(c => c.ToDto()).ToList()
        );
    }
}
