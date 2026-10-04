namespace CatSitter.Api.Entities;

public class Cat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public int AgeYears { get; set; } = 1;
    public int AgeMonths { get; set; } = 0;
    public decimal? Weight { get; set; }
    public string Gender { get; set; } = "male"; // male | female
    public bool IsNeutered { get; set; } = false;
    public string? PhotoUrl { get; set; }
    public string? DietaryHabits { get; set; }
    public string? MedicalNotes { get; set; }
    public List<string> Vaccines { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? Owner { get; set; }
}
