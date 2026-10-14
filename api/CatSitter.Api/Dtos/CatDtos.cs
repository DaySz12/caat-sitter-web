namespace CatSitter.Api.Dtos;

public record CatDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string? Breed,
    int AgeYears,
    int AgeMonths,
    decimal? Weight,
    string Gender,
    bool IsNeutered,
    string? PhotoUrl,
    string? DietaryHabits,
    string? MedicalNotes,
    string? EmergencyVetClinic,
    string? EmergencyContactPhone,
    List<string> Vaccines,
    DateTimeOffset CreatedAt
);

public record CatUpsertRequest(
    string Name,
    string? Breed,
    int AgeYears,
    int AgeMonths,
    decimal? Weight,
    string Gender,
    bool IsNeutered,
    string? PhotoUrl,
    string? DietaryHabits,
    string? MedicalNotes,
    string? EmergencyVetClinic,
    string? EmergencyContactPhone,
    List<string>? Vaccines
);
