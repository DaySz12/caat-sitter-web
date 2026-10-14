using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Repositories.Cats;

namespace CatSitter.Api.Services.Cats;

public class CatService : ICatService
{
    private readonly ICatRepository _catRepo;

    public CatService(ICatRepository catRepo)
    {
        _catRepo = catRepo;
    }

    public async Task<List<CatDto>> GetMineAsync(Guid ownerId)
    {
        var cats = await _catRepo.GetMineAsync(ownerId);
        return cats.Select(c => c.ToDto()).ToList();
    }

    public async Task<CatDto?> GetByIdAsync(Guid id)
    {
        var cat = await _catRepo.GetByIdAsync(id);
        return cat?.ToDto();
    }

    public async Task<CatDto> CreateAsync(Guid ownerId, CatUpsertRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            throw new ArgumentException("กรุณาระบุชื่อแมว");

        var cat = new Cat
        {
            OwnerId = ownerId,
            Name = req.Name.Trim(),
            Breed = req.Breed?.Trim(),
            AgeYears = req.AgeYears,
            AgeMonths = req.AgeMonths,
            Weight = req.Weight,
            Gender = req.Gender,
            IsNeutered = req.IsNeutered,
            PhotoUrl = req.PhotoUrl,
            DietaryHabits = req.DietaryHabits,
            MedicalNotes = req.MedicalNotes,
            EmergencyVetClinic = req.EmergencyVetClinic?.Trim(),
            EmergencyContactPhone = req.EmergencyContactPhone?.Trim(),
            Vaccines = req.Vaccines ?? new List<string>(),
        };

        await _catRepo.AddAsync(cat);
        await _catRepo.SaveChangesAsync();
        return cat.ToDto();
    }

    public async Task<CatDto> UpdateAsync(Guid id, Guid ownerId, CatUpsertRequest req)
    {
        var cat = await _catRepo.GetByIdAsync(id);
        if (cat is null)
            throw new KeyNotFoundException("ไม่พบข้อมูลแมว");

        if (cat.OwnerId != ownerId)
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์แก้ไขข้อมูลแมวตัวนี้");

        cat.Name = req.Name.Trim();
        cat.Breed = req.Breed?.Trim();
        cat.AgeYears = req.AgeYears;
        cat.AgeMonths = req.AgeMonths;
        cat.Weight = req.Weight;
        cat.Gender = req.Gender;
        cat.IsNeutered = req.IsNeutered;
        cat.PhotoUrl = req.PhotoUrl;
        cat.DietaryHabits = req.DietaryHabits;
        cat.MedicalNotes = req.MedicalNotes;
        cat.EmergencyVetClinic = req.EmergencyVetClinic?.Trim();
        cat.EmergencyContactPhone = req.EmergencyContactPhone?.Trim();
        cat.Vaccines = req.Vaccines ?? cat.Vaccines;
        cat.UpdatedAt = DateTimeOffset.UtcNow;

        await _catRepo.SaveChangesAsync();
        return cat.ToDto();
    }

    public async Task DeleteAsync(Guid id, Guid ownerId)
    {
        var cat = await _catRepo.GetByIdAsync(id);
        if (cat is null)
            throw new KeyNotFoundException("ไม่พบข้อมูลแมว");

        if (cat.OwnerId != ownerId)
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์ลบข้อมูลแมวตัวนี้");

        _catRepo.Remove(cat);
        await _catRepo.SaveChangesAsync();
    }
}
