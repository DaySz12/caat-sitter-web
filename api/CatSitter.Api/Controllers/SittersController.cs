using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/sitters")]
public class SittersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SittersController(AppDbContext db)
    {
        _db = db;
    }

    /// Verified & available sitters, ranked by the same Smart Matching score used on mobile:
    /// rating 40% + GPS proximity 35% + review volume 15% + price 10%.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SitterDto>>> Search([FromQuery] SitterSearchQuery q)
    {
        var query = _db.SitterProfiles
            .Include(s => s.User)
            .Where(s => s.IsVerified && s.IsAvailable)
            .AsQueryable();

        if (q.MaxPrice is > 0) query = query.Where(s => s.PricePerDay <= q.MaxPrice);
        if (q.MinRating is > 0) query = query.Where(s => s.RatingAvg >= q.MinRating);

        var sitters = await query.ToListAsync();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLowerInvariant();
            sitters = sitters.Where(s =>
                (s.User?.Name.ToLowerInvariant().Contains(term) ?? false) ||
                (s.Address?.ToLowerInvariant().Contains(term) ?? false) ||
                (s.Bio?.ToLowerInvariant().Contains(term) ?? false)
            ).ToList();
        }

        if (q.RequiredSkills is { Count: > 0 })
        {
            sitters = sitters.Where(s =>
            {
                var skills = MatchingService.ExtractSkillsFromBio(s.Bio);
                return q.RequiredSkills.All(skills.Contains);
            }).ToList();
        }

        // Date-range availability: drop sitters already at capacity for overlapping bookings.
        if (q.StartDate is { } start && q.EndDate is { } end)
        {
            var overlapping = await _db.Bookings
                .Where(b => (b.Status == "confirmed" || b.Status == "in_progress")
                            && b.StartDate <= end && b.EndDate >= start)
                .Select(b => b.SitterId)
                .ToListAsync();

            var busyCounts = overlapping
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            sitters = sitters.Where(s =>
                !busyCounts.TryGetValue(s.Id, out var busy) || busy < s.MaxCatsPerDay
            ).ToList();
        }

        var ranked = sitters.Select(s =>
        {
            double? distance = null;
            if (q.UserLat is { } lat && q.UserLng is { } lng && s.Latitude is { } sLat && s.Longitude is { } sLng)
            {
                distance = MatchingService.CalculateDistanceKm(lat, lng, sLat, sLng);
            }

            var score = MatchingService.ComputeMatchScore(s.RatingAvg, distance, s.ReviewCount, s.PricePerDay);
            return s.ToDto(distance, score);
        }).ToList();

        ranked = q.SortBy switch
        {
            "distance" => ranked.OrderBy(s => s.DistanceKm ?? double.MaxValue).ToList(),
            "rating" => ranked.OrderByDescending(s => s.RatingAvg).ToList(),
            "price_asc" => ranked.OrderBy(s => s.PricePerDay).ToList(),
            _ => ranked.OrderByDescending(s => s.MatchScore ?? 0).ToList(),
        };

        return Ok(ranked);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<SitterDto>> GetById(Guid id)
    {
        var sitter = await _db.SitterProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sitter is null) return NotFound();
        return Ok(sitter.ToDto());
    }

    [Authorize(Roles = "sitter")]
    [HttpPut("me")]
    public async Task<ActionResult<SitterDto>> UpdateMine(UpdateSitterProfileRequest req)
    {
        var sitterId = User.GetUserId();
        var profile = await _db.SitterProfiles.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == sitterId);
        if (profile is null) return NotFound();

        if (req.Bio is not null) profile.Bio = req.Bio;
        if (req.PricePerDay is not null) profile.PricePerDay = req.PricePerDay.Value;
        if (req.Address is not null) profile.Address = req.Address;
        if (req.Latitude is not null) profile.Latitude = req.Latitude;
        if (req.Longitude is not null) profile.Longitude = req.Longitude;
        if (req.ExperienceYears is not null) profile.ExperienceYears = req.ExperienceYears.Value;
        if (req.MaxCatsPerDay is not null) profile.MaxCatsPerDay = req.MaxCatsPerDay.Value;
        if (req.ServicePhotos is not null) profile.ServicePhotos = req.ServicePhotos;
        if (req.IsAvailable is not null) profile.IsAvailable = req.IsAvailable.Value;
        if (req.SocialLine is not null) profile.SocialLine = req.SocialLine;
        if (req.SocialFacebook is not null) profile.SocialFacebook = req.SocialFacebook;
        if (req.SocialInstagram is not null) profile.SocialInstagram = req.SocialInstagram;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(profile.ToDto());
    }
}
