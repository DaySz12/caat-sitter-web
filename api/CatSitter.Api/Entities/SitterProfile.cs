namespace CatSitter.Api.Entities;

public class SitterProfile
{
    public Guid Id { get; set; } // shares PK with User
    public string? Bio { get; set; }
    public decimal PricePerDay { get; set; } = 300.00m;
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int ExperienceYears { get; set; } = 1;
    public int MaxCatsPerDay { get; set; } = 3;
    public List<string> ServicePhotos { get; set; } = new();
    public string? IdCardUrl { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public decimal RatingAvg { get; set; } = 5.00m;
    public int ReviewCount { get; set; } = 0;
    public string? SocialLine { get; set; }
    public string? SocialFacebook { get; set; }
    public string? SocialInstagram { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}
