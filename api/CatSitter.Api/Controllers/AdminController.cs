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
        var pendingPayments = bookingStats.Count(b => b.PaymentStatus == "pending_verification");
        var pendingPayouts = await _db.PayoutRequests.CountAsync(p => p.Status == "pending");

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

        return Ok(new AdminDashboardDto(totalUsers, totalRevenue, pendingSitters, pendingPayments, pendingPayouts, bookingStats.Count, activeBookings, recent));
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
                b.Status, b.PaymentStatus, b.PaymentSlipUrl, b.PaymentSlipUploadedAt, b.PaymentRejectionReason,
                b.SpecialInstructions, b.CreatedAt,
                b.Owner == null ? null : new AdminUserContactDto(b.Owner.Name, b.Owner.Email, b.Owner.Phone, b.Owner.AvatarUrl),
                b.Sitter == null ? null : new AdminUserContactDto(b.Sitter.Name, b.Sitter.Email, b.Sitter.Phone, b.Sitter.AvatarUrl)
            ))
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<List<AdminBookingDto>>> GetPayments([FromQuery] string status = "pending_verification")
    {
        var query = _db.Bookings.Include(b => b.Owner).Include(b => b.Sitter).AsQueryable();
        if (status == "pending_verification")
        {
            query = query.Where(b => b.PaymentStatus == "pending_verification");
        }
        else if (status != "all")
        {
            query = query.Where(b => b.PaymentStatus == status);
        }
        else
        {
            query = query.Where(b => b.PaymentSlipUrl != null);
        }

        var payments = await query
            .OrderByDescending(b => b.PaymentSlipUploadedAt ?? b.CreatedAt)
            .Select(b => new AdminBookingDto(
                b.Id, b.BookingCode, b.StartDate, b.EndDate, b.DaysCount, b.BasePrice, b.ServiceFee, b.TotalPrice,
                b.Status, b.PaymentStatus, b.PaymentSlipUrl, b.PaymentSlipUploadedAt, b.PaymentRejectionReason,
                b.SpecialInstructions, b.CreatedAt,
                b.Owner == null ? null : new AdminUserContactDto(b.Owner.Name, b.Owner.Email, b.Owner.Phone, b.Owner.AvatarUrl),
                b.Sitter == null ? null : new AdminUserContactDto(b.Sitter.Name, b.Sitter.Email, b.Sitter.Phone, b.Sitter.AvatarUrl)
            ))
            .ToListAsync();

        return Ok(payments);
    }

    [HttpPost("bookings/{id:guid}/payment/approve")]
    public async Task<IActionResult> ApprovePayment(Guid id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound(new { message = "ไม่พบรายการจอง" });

        booking.PaymentStatus = "paid";
        booking.PaymentRejectionReason = null;
        if (booking.Status == "pending") booking.Status = "confirmed";
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        _db.Notifications.Add(new Notification
        {
            UserId = booking.OwnerId,
            Title = "✅ ยืนยันการชำระเงินเรียบร้อยแล้ว",
            Body = $"การชำระเงินสำหรับการจอง {booking.BookingCode} ยอด {booking.TotalPrice:N2} บาท ได้รับการยืนยันแล้ว",
            Type = "payment_approved",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        _db.Notifications.Add(new Notification
        {
            UserId = booking.SitterId,
            Title = "💰 เจ้าของแมวชำระเงินแล้ว",
            Body = $"การจอง {booking.BookingCode} ชำระเงินเรียบร้อยแล้ว เตรียมเริ่มดูแลน้องแมวได้เลย",
            Type = "payment_approved",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bookings/{id:guid}/payment/reject")]
    public async Task<IActionResult> RejectPayment(Guid id, AdminRejectPaymentRequest req)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound(new { message = "ไม่พบรายการจอง" });

        var reason = string.IsNullOrWhiteSpace(req.Reason)
            ? "สลิปไม่ชัดเจนหรือยอดเงินไม่ถูกต้อง"
            : req.Reason.Trim();

        booking.PaymentStatus = "rejected";
        booking.PaymentRejectionReason = reason;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        _db.Notifications.Add(new Notification
        {
            UserId = booking.OwnerId,
            Title = "⚠️ สลิปการชำระเงินไม่ผ่านการตรวจสอบ",
            Body = $"เหตุผล: {reason} กรุณาตรวจสอบและแนบสลิปใหม่อีกครั้ง",
            Type = "payment_rejected",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _db.SaveChangesAsync();
        return NoContent();
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

    [HttpGet("payouts")]
    public async Task<ActionResult<List<AdminPayoutDto>>> GetPayouts([FromQuery] string status = "all")
    {
        var query = _db.PayoutRequests.Include(p => p.Sitter).AsQueryable();

        if (status != "all")
        {
            query = query.Where(p => p.Status == status);
        }

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminPayoutDto(
                p.Id,
                p.SitterId,
                p.Amount,
                p.BankName,
                p.BankAccountNumber,
                p.BankAccountName,
                p.PromptPayNumber,
                p.Status,
                p.AdminNotes,
                p.TransferSlipUrl,
                p.CreatedAt,
                p.ProcessedAt,
                p.Sitter == null ? null : new AdminUserContactDto(p.Sitter.Name, p.Sitter.Email, p.Sitter.Phone, p.Sitter.AvatarUrl)
            ))
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("payouts/{id:guid}/approve")]
    public async Task<IActionResult> ApprovePayout(Guid id, AdminApprovePayoutRequest? req)
    {
        var payout = await _db.PayoutRequests.Include(p => p.Sitter).FirstOrDefaultAsync(p => p.Id == id);
        if (payout is null) return NotFound(new { message = "ไม่พบคำขอถอนเงินนี้" });

        if (payout.Status != "pending")
            return BadRequest(new { message = "คำขอนี้ได้รับการประมวลผลไปแล้ว" });

        payout.Status = "completed";
        payout.TransferSlipUrl = req?.TransferSlipUrl;
        payout.ProcessedAt = DateTimeOffset.UtcNow;

        _db.Notifications.Add(new Notification
        {
            UserId = payout.SitterId,
            Title = "โอนเงินเข้าบัญชีสำเร็จ 🎉",
            Body = $"ยอดเงิน ฿{payout.Amount:N0} ได้รับการโอนเข้าบัญชี {payout.BankName} ({payout.BankAccountNumber}) เรียบร้อยแล้ว",
            Type = "payout_completed",
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "อนุมัติและบันทึกการโอนเงินสำเร็จ" });
    }

    [HttpPost("payouts/{id:guid}/reject")]
    public async Task<IActionResult> RejectPayout(Guid id, AdminRejectPayoutRequest req)
    {
        var payout = await _db.PayoutRequests.Include(p => p.Sitter).FirstOrDefaultAsync(p => p.Id == id);
        if (payout is null) return NotFound(new { message = "ไม่พบคำขอถอนเงินนี้" });

        if (payout.Status != "pending")
            return BadRequest(new { message = "คำขอนี้ได้รับการประมวลผลไปแล้ว" });

        payout.Status = "rejected";
        payout.AdminNotes = req.Reason;
        payout.ProcessedAt = DateTimeOffset.UtcNow;

        // Refund the amount back to sitter's wallet balance
        if (payout.Sitter is not null)
        {
            payout.Sitter.WalletBalance += payout.Amount;
            payout.Sitter.UpdatedAt = DateTimeOffset.UtcNow;
        }

        _db.Notifications.Add(new Notification
        {
            UserId = payout.SitterId,
            Title = "คำขอถอนเงินถูกปฏิเสธ",
            Body = $"คำขอถอนเงิน ฿{payout.Amount:N0} ถูกปฏิเสธ (เหตุผล: {req.Reason}) ระบบได้คืนยอดเงินกลับเข้ากระเป๋าของคุณแล้ว",
            Type = "payout_rejected",
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "ปฏิเสธคำขอและคืนยอดเงินให้พี่เลี้ยงเรียบร้อยแล้ว" });
    }

    [HttpGet("settings/payment-info")]
    public async Task<ActionResult<PlatformPaymentSettingDto>> GetPlatformPaymentSetting()
    {
        var setting = await _db.PlatformPaymentSettings.FirstOrDefaultAsync();
        if (setting is null)
        {
            return Ok(new PlatformPaymentSettingDto(
                "กสิกรไทย (KBANK)",
                "123-2-88990-0",
                "บจก. แคท ซิตเตอร์ (ไทยแลนด์)",
                "010-5-56600-0000",
                "บจก. แคท ซิตเตอร์ (ไทยแลนด์)"
            ));
        }

        return Ok(new PlatformPaymentSettingDto(
            setting.BankName,
            setting.BankAccountNumber,
            setting.BankAccountName,
            setting.PromptPayNumber,
            setting.PromptPayName
        ));
    }

    [HttpPut("settings/payment-info")]
    public async Task<ActionResult<PlatformPaymentSettingDto>> UpdatePlatformPaymentSetting(UpdatePlatformPaymentSettingRequest req)
    {
        var setting = await _db.PlatformPaymentSettings.FirstOrDefaultAsync();
        if (setting is null)
        {
            setting = new PlatformPaymentSetting
            {
                BankName = req.BankName.Trim(),
                BankAccountNumber = req.BankAccountNumber.Trim(),
                BankAccountName = req.BankAccountName.Trim(),
                PromptPayNumber = req.PromptPayNumber.Trim(),
                PromptPayName = req.PromptPayName.Trim(),
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.PlatformPaymentSettings.Add(setting);
        }
        else
        {
            setting.BankName = req.BankName.Trim();
            setting.BankAccountNumber = req.BankAccountNumber.Trim();
            setting.BankAccountName = req.BankAccountName.Trim();
            setting.PromptPayNumber = req.PromptPayNumber.Trim();
            setting.PromptPayName = req.PromptPayName.Trim();
            setting.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new PlatformPaymentSettingDto(
            setting.BankName,
            setting.BankAccountNumber,
            setting.BankAccountName,
            setting.PromptPayNumber,
            setting.PromptPayName
        ));
    }
}
