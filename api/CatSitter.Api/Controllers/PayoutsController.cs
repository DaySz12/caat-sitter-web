using System.Security.Claims;
using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/payouts")]
[Authorize]
public class PayoutsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PayoutsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<ActionResult<List<PayoutDto>>> GetMyPayouts()
    {
        var uid = CurrentUserId;
        var user = await _db.Users.FindAsync(uid);
        if (user is null) return Unauthorized();

        var list = await _db.PayoutRequests
            .Where(p => p.SitterId == uid)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PayoutDto(
                p.Id,
                p.SitterId,
                user.Name,
                user.AvatarUrl,
                p.Amount,
                p.BankName,
                p.BankAccountNumber,
                p.BankAccountName,
                p.PromptPayNumber,
                p.Status,
                p.AdminNotes,
                p.TransferSlipUrl,
                p.CreatedAt,
                p.ProcessedAt
            ))
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<PayoutDto>> RequestPayout(CreatePayoutRequest req)
    {
        var uid = CurrentUserId;
        var user = await _db.Users.FindAsync(uid);
        if (user is null) return Unauthorized();
        if (user.Role != "sitter")
            return BadRequest(new { message = "เฉพาะพี่เลี้ยงเท่านั้นที่สามารถขอถอนเงินได้" });

        if (req.Amount < 50m)
            return BadRequest(new { message = "ยอดเงินขั้นต่ำในการถอนคือ 50 บาท" });

        if (user.WalletBalance < req.Amount)
            return BadRequest(new { message = $"ยอดเงินในกระเป๋าของคุณไม่เพียงพอ (คงเหลือ ฿{user.WalletBalance:N0})" });

        if (string.IsNullOrWhiteSpace(req.BankName) || string.IsNullOrWhiteSpace(req.BankAccountNumber) || string.IsNullOrWhiteSpace(req.BankAccountName))
            return BadRequest(new { message = "กรุณากรอกข้อมูลธนาคารและเลขบัญชีให้ครบถ้วน" });

        // Deduct from wallet balance to reserve funds
        user.WalletBalance -= req.Amount;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var payout = new PayoutRequest
        {
            SitterId = uid,
            Amount = req.Amount,
            BankName = req.BankName.Trim(),
            BankAccountNumber = req.BankAccountNumber.Trim(),
            BankAccountName = req.BankAccountName.Trim(),
            PromptPayNumber = string.IsNullOrWhiteSpace(req.PromptPayNumber) ? null : req.PromptPayNumber.Trim(),
            Status = "pending",
        };

        _db.PayoutRequests.Add(payout);

        _db.Notifications.Add(new Notification
        {
            UserId = uid,
            Title = "ยื่นคำขอถอนเงินแล้ว",
            Body = $"คำขอถอนเงินจำนวน ฿{req.Amount:N0} เข้าบัญชี {payout.BankName} ({payout.BankAccountNumber}) ถูกส่งให้เจ้าหน้าที่ตรวจสอบแล้ว",
            Type = "payout_requested",
        });

        await _db.SaveChangesAsync();

        return Ok(new PayoutDto(
            payout.Id,
            payout.SitterId,
            user.Name,
            user.AvatarUrl,
            payout.Amount,
            payout.BankName,
            payout.BankAccountNumber,
            payout.BankAccountName,
            payout.PromptPayNumber,
            payout.Status,
            payout.AdminNotes,
            payout.TransferSlipUrl,
            payout.CreatedAt,
            payout.ProcessedAt
        ));
    }
}
