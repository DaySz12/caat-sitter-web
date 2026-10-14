using CatSitter.Api.Entities;

namespace CatSitter.Api.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<Booking?> GetWithDetailsByIdAsync(Guid id);
    Task<List<Booking>> GetMineAsync(Guid userId, string? status);
    Task<Dictionary<Guid, Review>> GetReviewsForBookingsAsync(List<Guid> bookingIds);
    Task AddAsync(Booking booking);
    Task<SitterProfile?> GetSitterProfileAsync(Guid sitterId);
    Task<List<Cat>> GetCatsByIdsAndOwnerAsync(List<Guid> catIds, Guid ownerId);
    Task<User?> GetUserAsync(Guid userId);
    Task<List<AttendanceRecord>> GetOpenVisitsByBookingIdAsync(Guid bookingId);
    Task<List<Guid>> GetAdminUserIdsAsync();
    void AddNotification(Notification notification);
    Task SaveChangesAsync();
}
