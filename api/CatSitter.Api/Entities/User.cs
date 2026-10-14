namespace CatSitter.Api.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "owner"; // owner | sitter | admin
    public string Status { get; set; } = "approved"; // pending | approved | rejected | suspended
    public string? RejectionReason { get; set; }
    public decimal WalletBalance { get; set; } = 0m;
    public string? Address { get; set; }
    public string? BuildingNote { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LineId { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? EmergencyContactName { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public SitterProfile? SitterProfile { get; set; }
    public ICollection<Cat> Cats { get; set; } = new List<Cat>();
}
