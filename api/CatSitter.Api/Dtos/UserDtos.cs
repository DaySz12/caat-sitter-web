namespace CatSitter.Api.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string Name,
    string? Phone,
    string? AvatarUrl,
    string Role,
    string Status,
    decimal WalletBalance,
    DateTimeOffset CreatedAt,
    string? Address = null,
    string? BuildingNote = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LineId = null,
    string? SecondaryPhone = null,
    string? EmergencyContactName = null
);

public record UpdateProfileRequest(
    string? Name,
    string? Phone,
    string? AvatarUrl,
    string? Address = null,
    string? BuildingNote = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LineId = null,
    string? SecondaryPhone = null,
    string? EmergencyContactName = null
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
