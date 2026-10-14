using System.Text.Json.Serialization;

namespace CatSitter.Api.Dtos;

// These admin DTOs intentionally serialize as snake_case (matching the old Supabase column
// names) so the admin Vue app's existing templates can stay almost untouched — only the
// data-fetching calls in each view's <script> were swapped from supabase-js to this API.

public record AdminUserBriefDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("avatar_url")] string? AvatarUrl
);

public record AdminUserContactDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("avatar_url")] string? AvatarUrl
);

public record AdminRecentBookingDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("booking_code")] string BookingCode,
    [property: JsonPropertyName("start_date")] DateOnly StartDate,
    [property: JsonPropertyName("end_date")] DateOnly EndDate,
    [property: JsonPropertyName("total_price")] decimal TotalPrice,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("payment_status")] string PaymentStatus,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("owner")] AdminUserBriefDto? Owner,
    [property: JsonPropertyName("sitter")] AdminUserBriefDto? Sitter
);

public record AdminDashboardDto(
    [property: JsonPropertyName("total_users")] int TotalUsers,
    [property: JsonPropertyName("total_revenue")] decimal TotalRevenue,
    [property: JsonPropertyName("pending_sitters")] int PendingSitters,
    [property: JsonPropertyName("pending_payments")] int PendingPayments,
    [property: JsonPropertyName("pending_payouts")] int PendingPayouts,
    [property: JsonPropertyName("total_bookings")] int TotalBookings,
    [property: JsonPropertyName("active_bookings")] int ActiveBookings,
    [property: JsonPropertyName("recent_bookings")] List<AdminRecentBookingDto> RecentBookings
);

public record AdminPayoutDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("sitter_id")] Guid SitterId,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("bank_name")] string BankName,
    [property: JsonPropertyName("bank_account_number")] string BankAccountNumber,
    [property: JsonPropertyName("bank_account_name")] string BankAccountName,
    [property: JsonPropertyName("promptpay_number")] string? PromptPayNumber,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("admin_notes")] string? AdminNotes,
    [property: JsonPropertyName("transfer_slip_url")] string? TransferSlipUrl,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("processed_at")] DateTimeOffset? ProcessedAt,
    [property: JsonPropertyName("sitter")] AdminUserContactDto? Sitter
);

public record AdminSitterProfileBriefDto(
    [property: JsonPropertyName("bio")] string? Bio,
    [property: JsonPropertyName("price_per_day")] decimal PricePerDay,
    [property: JsonPropertyName("experience_years")] int ExperienceYears,
    [property: JsonPropertyName("max_cats_per_day")] int MaxCatsPerDay,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("id_card_url")] string? IdCardUrl,
    [property: JsonPropertyName("service_photos")] List<string> ServicePhotos,
    [property: JsonPropertyName("is_verified")] bool IsVerified
);

public record AdminSitterDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("avatar_url")] string? AvatarUrl,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("rejection_reason")] string? RejectionReason,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("profile")] AdminSitterProfileBriefDto? Profile
);

public record AdminRejectSitterRequest(string? Reason);
public record AdminRejectPaymentRequest([property: JsonPropertyName("reason")] string? Reason);

public record AdminBookingDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("booking_code")] string BookingCode,
    [property: JsonPropertyName("start_date")] DateOnly StartDate,
    [property: JsonPropertyName("end_date")] DateOnly EndDate,
    [property: JsonPropertyName("days_count")] int DaysCount,
    [property: JsonPropertyName("base_price")] decimal BasePrice,
    [property: JsonPropertyName("service_fee")] decimal ServiceFee,
    [property: JsonPropertyName("total_price")] decimal TotalPrice,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("payment_status")] string PaymentStatus,
    [property: JsonPropertyName("payment_slip_url")] string? PaymentSlipUrl,
    [property: JsonPropertyName("payment_slip_uploaded_at")] DateTimeOffset? PaymentSlipUploadedAt,
    [property: JsonPropertyName("payment_rejection_reason")] string? PaymentRejectionReason,
    [property: JsonPropertyName("special_instructions")] string? SpecialInstructions,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("owner")] AdminUserContactDto? Owner,
    [property: JsonPropertyName("sitter")] AdminUserContactDto? Sitter
);

public record AdminUserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("avatar_url")] string? AvatarUrl,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);

public record AdminUpdateUserStatusRequest(string Status);

public record AdminServiceFeeDto([property: JsonPropertyName("percentage")] decimal Percentage);

public record AdminUpdateServiceFeeRequest(decimal Percentage);
