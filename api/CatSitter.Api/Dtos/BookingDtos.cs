namespace CatSitter.Api.Dtos;

public record BookingDto(
    Guid Id,
    string BookingCode,
    Guid OwnerId,
    Guid SitterId,
    DateOnly StartDate,
    DateOnly EndDate,
    int DaysCount,
    decimal BasePrice,
    decimal ServiceFee,
    decimal TotalPrice,
    string Status,
    string PaymentStatus,
    string? PaymentSlipUrl,
    DateTimeOffset? PaymentSlipUploadedAt,
    string? PaymentRejectionReason,
    string? SpecialInstructions,
    string? CancellationReason,
    DateTimeOffset CreatedAt,
    UserDto? Owner,
    UserDto? Sitter,
    List<CatDto> Cats,
    ReviewDto? Review = null
);

public record CreateBookingRequest(
    Guid SitterId,
    List<Guid> CatIds,
    DateOnly StartDate,
    DateOnly EndDate,
    string? SpecialInstructions
);

public record UpdateBookingStatusRequest(
    string Status, // confirmed | in_progress | completed | cancelled | rejected
    string? CancellationReason
);

public record UploadPaymentSlipRequest(
    string SlipUrl
);

