using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
public class BannersController : ControllerBase
{
    private readonly AppDbContext _db;

    public BannersController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Public endpoint for client web: returns only active banners/popups
    /// </summary>
    [HttpGet("api/banners")]
    public async Task<ActionResult<List<BannerDto>>> GetActiveBanners([FromQuery] string? type)
    {
        var query = _db.Banners.Where(b => b.IsActive);
        if (!string.IsNullOrWhiteSpace(type) && type != "all")
        {
            query = query.Where(b => b.Type == type.ToLower());
        }

        var banners = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(banners.Select(b => b.ToDto()).ToList());
    }

    /// <summary>
    /// Admin endpoint: returns all banners/popups with full management data
    /// </summary>
    [HttpGet("api/admin/banners")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<List<AdminBannerDto>>> GetAllForAdmin([FromQuery] string? type)
    {
        var query = _db.Banners.AsQueryable();
        if (!string.IsNullOrWhiteSpace(type) && type != "all")
        {
            query = query.Where(b => b.Type == type.ToLower());
        }

        var banners = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(banners.Select(b => b.ToAdminDto()).ToList());
    }

    [HttpPost("api/admin/banners")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<AdminBannerDto>> Create(CreateBannerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.ImageUrl))
        {
            return BadRequest(new { message = "กรุณากรอกชื่อและรูปภาพแบนเนอร์/ป๊อปอัปให้ครบถ้วน" });
        }

        var banner = new Banner
        {
            Type = req.Type?.ToLower() == "popup" ? "popup" : "banner",
            Title = req.Title.Trim(),
            ImageUrl = req.ImageUrl.Trim(),
            LinkUrl = string.IsNullOrWhiteSpace(req.LinkUrl) ? null : req.LinkUrl.Trim(),
            IsActive = req.IsActive ?? true,
            DisplayOrder = req.DisplayOrder ?? 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllForAdmin), new { id = banner.Id }, banner.ToAdminDto());
    }

    [HttpPut("api/admin/banners/{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<AdminBannerDto>> Update(Guid id, UpdateBannerRequest req)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner is null) return NotFound(new { message = "ไม่พบแบนเนอร์ที่ต้องการแก้ไข" });

        if (!string.IsNullOrWhiteSpace(req.Type)) banner.Type = req.Type.ToLower() == "popup" ? "popup" : "banner";
        if (!string.IsNullOrWhiteSpace(req.Title)) banner.Title = req.Title.Trim();
        if (!string.IsNullOrWhiteSpace(req.ImageUrl)) banner.ImageUrl = req.ImageUrl.Trim();
        if (req.LinkUrl is not null) banner.LinkUrl = string.IsNullOrWhiteSpace(req.LinkUrl) ? null : req.LinkUrl.Trim();
        if (req.IsActive.HasValue) banner.IsActive = req.IsActive.Value;
        if (req.DisplayOrder.HasValue) banner.DisplayOrder = req.DisplayOrder.Value;

        banner.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(banner.ToAdminDto());
    }

    [HttpDelete("api/admin/banners/{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner is null) return NotFound(new { message = "ไม่พบแบนเนอร์ที่ต้องการลบ" });

        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("api/admin/banners/{id:guid}/toggle")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<AdminBannerDto>> ToggleActive(Guid id)
    {
        var banner = await _db.Banners.FindAsync(id);
        if (banner is null) return NotFound(new { message = "ไม่พบแบนเนอร์" });

        banner.IsActive = !banner.IsActive;
        banner.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(banner.ToAdminDto());
    }
}
