using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<UserSummaryDto> GetUserSummaryAsync(Guid userId)
    {
        var unreadNotifications = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        var unreadMessages = await _db.Messages.CountAsync(m =>
            !m.IsRead && m.SenderId != userId &&
            _db.ChatRooms.Any(r => r.Id == m.RoomId && (r.OwnerId == userId || r.SitterId == userId)));

        var bookings = await _db.Bookings
            .Where(b => b.OwnerId == userId || b.SitterId == userId)
            .Select(b => new { b.SitterId, b.Status, b.EndDate, b.BasePrice, b.TotalPrice })
            .ToListAsync();

        var completedJobs = bookings.Where(b => b.SitterId == userId && b.Status == "completed").ToList();
        var today = BangkokTime.Today;

        return new UserSummaryDto(
            UnreadNotifications: unreadNotifications,
            UnreadMessages: unreadMessages,
            PendingRequests: bookings.Count(b => b.Status == "pending"),
            ActiveBookings: bookings.Count(b => b.Status is "confirmed" or "in_progress"),
            CompletedBookings: bookings.Count(b => b.Status == "completed"),
            TotalEarnings: completedJobs.Sum(b => b.BasePrice),
            MonthEarnings: completedJobs.Where(b => b.EndDate.Year == today.Year && b.EndDate.Month == today.Month).Sum(b => b.BasePrice),
            TotalSpent: bookings.Where(b => b.SitterId != userId && b.Status == "completed").Sum(b => b.TotalPrice)
        );
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
