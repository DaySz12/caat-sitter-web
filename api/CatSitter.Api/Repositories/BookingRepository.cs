using CatSitter.Api.Data;
using CatSitter.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Booking> BaseQuery() => _db.Bookings
        .Include(b => b.Owner)
        .Include(b => b.Sitter)
        .Include(b => b.BookingCats).ThenInclude(bc => bc.Cat);

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _db.Bookings.FindAsync(id);
    }

    public async Task<Booking?> GetWithDetailsByIdAsync(Guid id)
    {
        return await BaseQuery().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetMineAsync(Guid userId, string? status)
    {
        var query = BaseQuery().Where(b => b.OwnerId == userId || b.SitterId == userId);
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(b => b.Status == status);
        }
        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<Dictionary<Guid, Review>> GetReviewsForBookingsAsync(List<Guid> bookingIds)
    {
        return await _db.Reviews
            .Include(r => r.Reviewer)
            .Where(r => bookingIds.Contains(r.BookingId))
            .ToDictionaryAsync(r => r.BookingId);
    }

    public async Task AddAsync(Booking booking)
    {
        await _db.Bookings.AddAsync(booking);
    }

    public async Task<SitterProfile?> GetSitterProfileAsync(Guid sitterId)
    {
        return await _db.SitterProfiles.FindAsync(sitterId);
    }

    public async Task<List<Cat>> GetCatsByIdsAndOwnerAsync(List<Guid> catIds, Guid ownerId)
    {
        return await _db.Cats.Where(c => catIds.Contains(c.Id) && c.OwnerId == ownerId).ToListAsync();
    }

    public async Task<User?> GetUserAsync(Guid userId)
    {
        return await _db.Users.FindAsync(userId);
    }

    public async Task<List<AttendanceRecord>> GetOpenVisitsByBookingIdAsync(Guid bookingId)
    {
        return await _db.AttendanceRecords
            .Where(a => a.BookingId == bookingId && a.CheckOutTime == null)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetAdminUserIdsAsync()
    {
        return await _db.Users
            .Where(u => u.Role == "admin")
            .Select(u => u.Id)
            .ToListAsync();
    }

    public void AddNotification(Notification notification)
    {
        _db.Notifications.Add(notification);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
