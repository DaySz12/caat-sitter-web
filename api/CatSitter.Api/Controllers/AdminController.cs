using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> Dashboard()
    {
        var totalUsers = await _db.Users.CountAsync();
        var pendingSitters = await _db.Users.CountAsync(u => u.Role == "sitter" && u.Status == "pending");

        var bookingStats = await _db.Bookings
            .Select(b => new { b.TotalPrice, b.Status, b.PaymentStatus })
            .ToListAsync();

        var totalRevenue = bookingStats.Where(b => b.PaymentStatus == "paid").Sum(b => b.TotalPrice);
        var activeBookings = bookingStats.Count(b => b.Status == "confirmed" || b.Status == "in_progress");

        var recent = await _db.Bookings
            .Include(b => b.Owner)
            .Include(b => b.Sitter)
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .Select(b => new AdminRecentBookingDto(
                b.Id, b.BookingCode, b.StartDate, b.EndDate, b.TotalPrice, b.Status, b.PaymentStatus, b.CreatedAt,
                b.Owner == null ? null : new AdminUserBriefDto(b.Owner.Name, b.Owner.AvatarUrl),
                b.Sitter == null ? null : new AdminUserBriefDto(b.Sitter.Name, b.Sitter.AvatarUrl)
            ))
            .ToListAsync();

        return Ok(new AdminDashboardDto(totalUsers, totalRevenue, pendingSitters, bookingStats.Count, activeBookings, recent));
    }

    [HttpGet("sitters")]
    public async Task<ActionResult<List<AdminSitterDto>>> GetSitters([FromQuery] string status = "pending")
    {
        var sitters = await _db.Users
            .Where(u => u.Role == "sitter" && u.Status == status)
            .Include(u => u.SitterProfile)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminSitterDto(
                u.Id, u.Name, u.Email, u.Phone, u.AvatarUrl, u.Status, u.RejectionReason, u.CreatedAt,
                u.SitterProfile == null ? null : new AdminSitterProfileBriefDto(
                    u.SitterProfile.Bio, u.SitterProfile.PricePerDay, u.SitterProfile.ExperienceYears,
                    u.SitterProfile.MaxCatsPerDay, u.SitterProfile.Address, u.SitterProfile.IdCardUrl,
                    u.SitterProfile.ServicePhotos, u.SitterProfile.IsVerified
                )
            ))
            .ToListAsync();

        return Ok(sitters);
    }

    [HttpPost("sitters/{id:guid}/approve")]
    public async Task<IActionResult> ApproveSitter(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null || user.Role != "sitter") return NotFound();

        user.Status = "approved";
        user.RejectionReason = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var profile = await _db.SitterProfiles.FindAsync(id);
        if (profile is not null)
        {
            profile.IsVerified = true;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
        }

        _db.Notifications.Add(new Notification
        {
            UserId = id,
            Title = "🎉 บัญชีผู้รับเลี้ยงของคุณได้รับการอนุมัติแล้ว!",
            Body = "ยินดีด้วย! คุณสามารถเริ่มเปิดรับงานดูแลน้องแมวบนระบบ Cat Sitter ได้ทันที",
            Type = "sitter_approved",
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("sitters/{id:guid}/reject")]
    public async Task<IActionResult> RejectSitter(Guid id, AdminRejectSitterRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null || user.Role != "sitter") return NotFound();

        var reason = string.IsNullOrWhiteSpace(req.Reason)
            ? "เอกสารไม่ชัดเจนหรือไม่ผ่านเกณฑ์การตรวจสอบ"
            : req.Reason.Trim();

        user.Status = "rejected";
        user.RejectionReason = reason;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var profile = await _db.SitterProfiles.FindAsync(id);
        if (profile is not null)
        {
            profile.IsVerified = false;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
        }

        _db.Notifications.Add(new Notification
        {
            UserId = id,
            Title = "⚠️ บัญชีผู้รับเลี้ยงยังไม่ผ่านการอนุมัติ",
            Body = $"เหตุผล: {reason}",
            Type = "sitter_rejected",
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<List<AdminBookingDto>>> GetBookings([FromQuery] string status = "all")
    {
        var query = _db.Bookings.Include(b => b.Owner).Include(b => b.Sitter).AsQueryable();
        if (status != "all") query = query.Where(b => b.Status == status);

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new AdminBookingDto(
                b.Id, b.BookingCode, b.StartDate, b.EndDate, b.DaysCount, b.BasePrice, b.ServiceFee, b.TotalPrice,
                b.Status, b.PaymentStatus, b.SpecialInstructions, b.CreatedAt,
                b.Owner == null ? null : new AdminUserContactDto(b.Owner.Name, b.Owner.Email, b.Owner.Phone, b.Owner.AvatarUrl),
                b.Sitter == null ? null : new AdminUserContactDto(b.Sitter.Name, b.Sitter.Email, b.Sitter.Phone, b.Sitter.AvatarUrl)
            ))
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers([FromQuery] string role = "all")
    {
        var query = _db.Users.AsQueryable();
        if (role != "all") query = query.Where(u => u.Role == role);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto(u.Id, u.Name, u.Email, u.Phone, u.AvatarUrl, u.Role, u.Status, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, AdminUpdateUserStatusRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role == "admin") return Forbid();

        user.Status = req.Status;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("settings/service-fee")]
    public async Task<ActionResult<AdminServiceFeeDto>> GetServiceFee()
    {
        var fee = await _db.ServiceFees.Where(f => f.IsActive).FirstOrDefaultAsync();
        return Ok(new AdminServiceFeeDto(fee?.Percentage ?? 10.0m));
    }

    [HttpPut("settings/service-fee")]
    public async Task<ActionResult<AdminServiceFeeDto>> UpdateServiceFee(AdminUpdateServiceFeeRequest req)
    {
        var fee = await _db.ServiceFees.Where(f => f.IsActive).FirstOrDefaultAsync();
        if (fee is null)
        {
            fee = new ServiceFee { Name = "Standard Platform Fee", Percentage = req.Percentage, IsActive = true };
            _db.ServiceFees.Add(fee);
        }
        else
        {
            fee.Percentage = req.Percentage;
            fee.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new AdminServiceFeeDto(fee.Percentage));
    }
}
