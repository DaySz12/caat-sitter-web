using CatSitter.Api.Dtos;

namespace CatSitter.Api.Services.Bookings;

public interface IBookingService
{
    Task<List<BookingDto>> GetMineAsync(Guid userId, string? status);
    Task<BookingDto?> GetByIdAsync(Guid id, Guid userId, string role);
    Task<BookingDto> CreateAsync(Guid ownerId, CreateBookingRequest req);
    Task<BookingDto> UpdateStatusAsync(Guid id, Guid userId, string role, UpdateBookingStatusRequest req);
    Task<BookingDto> RequestRefundAsync(Guid id, Guid userId, string role, RequestRefundRequest req);
    Task<BookingDto> UploadPaymentSlipAsync(Guid id, Guid userId, string role, UploadPaymentSlipRequest req);
}
