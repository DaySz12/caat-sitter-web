using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(user.ToDto());
    }

    /// Badge counts and dashboard stats for the signed-in user, polled by the web app's sidebar.
    [HttpGet("me/summary")]
    public async Task<ActionResult<UserSummaryDto>> GetMySummary()
    {
        var userId = User.GetUserId();

        var unreadNotifications = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        var unreadMessages = await _db.Messages.CountAsync(m =>
            !m.IsRead && m.SenderId != userId &&
            _db.ChatRooms.Any(r => r.Id == m.RoomId && (r.OwnerId == userId || r.SitterId == userId)));

        var bookings = await _db.Bookings
            .Where(b => b.OwnerId == userId || b.SitterId == userId)
            .Select(b => new { b.SitterId, b.Status, b.EndDate, b.BasePrice, b.TotalPrice })
            .ToListAsync();

        // A sitter earns the base price; the platform keeps the service fee.
        var completedJobs = bookings.Where(b => b.SitterId == userId && b.Status == "completed").ToList();
        var today = BangkokTime.Today;

        return Ok(new UserSummaryDto(
            UnreadNotifications: unreadNotifications,
            UnreadMessages: unreadMessages,
            PendingRequests: bookings.Count(b => b.Status == "pending"),
            ActiveBookings: bookings.Count(b => b.Status is "confirmed" or "in_progress"),
            CompletedBookings: bookings.Count(b => b.Status == "completed"),
            TotalEarnings: completedJobs.Sum(b => b.BasePrice),
            MonthEarnings: completedJobs.Where(b => b.EndDate.Year == today.Year && b.EndDate.Month == today.Month).Sum(b => b.BasePrice),
            TotalSpent: bookings.Where(b => b.SitterId != userId && b.Status == "completed").Sum(b => b.TotalPrice)
        ));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequest req)
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        if (req.Name is not null) user.Name = req.Name;
        if (req.Phone is not null) user.Phone = req.Phone;
        if (req.AvatarUrl is not null) user.AvatarUrl = req.AvatarUrl;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(user.ToDto());
    }
}
