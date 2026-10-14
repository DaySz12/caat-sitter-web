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
    string? ServiceAddress,
    string? EmergencyContactPhone,
    string? PreferredVetClinic,
    string? RefundStatus,
    string? RefundReason,
    string? RefundBankInfo,
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
    string? SpecialInstructions,
    string? ServiceAddress = null,
    string? EmergencyContactPhone = null,
    string? PreferredVetClinic = null
);

public record UpdateBookingStatusRequest(
    string Status, // confirmed | in_progress | completed | cancelled | rejected
    string? CancellationReason
);

public record UploadPaymentSlipRequest(
    string SlipUrl
);

public record RequestRefundRequest(
    string Reason,
    string BankInfo
);

