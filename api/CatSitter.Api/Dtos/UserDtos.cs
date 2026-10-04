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
    DateTimeOffset CreatedAt
);

public record UpdateProfileRequest(
    string? Name,
    string? Phone,
    string? AvatarUrl
);
