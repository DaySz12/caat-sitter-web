namespace CatSitter.Api.Entities;

public class Banner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// "banner" (top slider) or "popup" (modal announcement)
    /// </summary>
    public string Type { get; set; } = "banner";
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
