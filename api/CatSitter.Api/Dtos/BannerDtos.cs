using System.Text.Json.Serialization;

namespace CatSitter.Api.Dtos;

// For owner/sitter web (camelCase)
public record BannerDto(
    Guid Id,
    string Type,
    string Title,
    string ImageUrl,
    string? LinkUrl,
    bool IsActive,
    int DisplayOrder,
    DateTimeOffset CreatedAt
);

// For admin portal (snake_case)
public record AdminBannerDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("image_url")] string ImageUrl,
    [property: JsonPropertyName("link_url")] string? LinkUrl,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("display_order")] int DisplayOrder,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt
);

public record CreateBannerRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("image_url")] string ImageUrl,
    [property: JsonPropertyName("link_url")] string? LinkUrl,
    [property: JsonPropertyName("is_active")] bool? IsActive,
    [property: JsonPropertyName("display_order")] int? DisplayOrder
);

public record UpdateBannerRequest(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("image_url")] string? ImageUrl,
    [property: JsonPropertyName("link_url")] string? LinkUrl,
    [property: JsonPropertyName("is_active")] bool? IsActive,
    [property: JsonPropertyName("display_order")] int? DisplayOrder
);
