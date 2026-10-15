namespace CatSitter.Api.Dtos;

public record SitterDto(
    Guid Id,
    string? Bio,
    decimal PricePerDay,
    string? Address,
    double? Latitude,
    double? Longitude,
    int ExperienceYears,
    int MaxCatsPerDay,
    List<string> ServicePhotos,
    bool IsVerified,
    bool IsAvailable,
    decimal RatingAvg,
    int ReviewCount,
    string? SocialLine,
    string? SocialFacebook,
    string? SocialInstagram,
    UserDto? User,
    double? DistanceKm,
    double? MatchScore,
    List<string> SpecialSkills
);

public record SitterSearchQuery(
    string? Search,
    decimal? MaxPrice,
    decimal? MinRating,
    double? UserLat,
    double? UserLng,
    string SortBy = "smart", // smart | distance | rating | price_asc
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    List<string>? RequiredSkills = null
);

public record UpdateSitterProfileRequest(
    string? Bio,
    decimal? PricePerDay,
    string? Address,
    double? Latitude,
    double? Longitude,
    int? ExperienceYears,
    int? MaxCatsPerDay,
    List<string>? ServicePhotos,
    bool? IsAvailable,
    string? SocialLine,
    string? SocialFacebook,
    string? SocialInstagram
);

public record SitterVerificationStatusDto(
    bool IsVerified,
    string Status,
    string? IdCardUrl,
    string? RejectionReason,
    DateTimeOffset? SubmittedAt
);

public record SubmitVerificationRequest(
    string IdCardUrl
);
