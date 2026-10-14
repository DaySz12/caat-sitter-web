using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/cats")]
[Authorize]
public class CatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatsController(AppDbContext db)
    {
        _db = db;
    }

    /// Cats belonging to the signed-in owner.
    [HttpGet("mine")]
    public async Task<ActionResult<List<CatDto>>> GetMine()
    {
        var ownerId = User.GetUserId();
        var cats = await _db.Cats
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(cats.Select(c => c.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatDto>> GetById(Guid id)
    {
        var cat = await _db.Cats.FindAsync(id);
        if (cat is null) return NotFound();
        return Ok(cat.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<CatDto>> Create(CatUpsertRequest req)
    {
        var ownerId = User.GetUserId();
        var cat = new Cat
        {
            OwnerId = ownerId,
            Name = req.Name,
            Breed = req.Breed,
            AgeYears = req.AgeYears,
            AgeMonths = req.AgeMonths,
            Weight = req.Weight,
            Gender = req.Gender,
            IsNeutered = req.IsNeutered,
            PhotoUrl = req.PhotoUrl,
            DietaryHabits = req.DietaryHabits,
            MedicalNotes = req.MedicalNotes,
            EmergencyVetClinic = req.EmergencyVetClinic,
            EmergencyContactPhone = req.EmergencyContactPhone,
            Vaccines = req.Vaccines ?? new List<string>(),
        };
        _db.Cats.Add(cat);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CatDto>> Update(Guid id, CatUpsertRequest req)
    {
        var ownerId = User.GetUserId();
        var cat = await _db.Cats.FindAsync(id);
        if (cat is null) return NotFound();
        if (cat.OwnerId != ownerId) return Forbid();

        cat.Name = req.Name;
        cat.Breed = req.Breed;
        cat.AgeYears = req.AgeYears;
        cat.AgeMonths = req.AgeMonths;
        cat.Weight = req.Weight;
        cat.Gender = req.Gender;
        cat.IsNeutered = req.IsNeutered;
        cat.PhotoUrl = req.PhotoUrl;
        cat.DietaryHabits = req.DietaryHabits;
        cat.MedicalNotes = req.MedicalNotes;
        cat.EmergencyVetClinic = req.EmergencyVetClinic;
        cat.EmergencyContactPhone = req.EmergencyContactPhone;
        cat.Vaccines = req.Vaccines ?? cat.Vaccines;
        cat.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(cat.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = User.GetUserId();
        var cat = await _db.Cats.FindAsync(id);
        if (cat is null) return NotFound();
        if (cat.OwnerId != ownerId) return Forbid();

        _db.Cats.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
