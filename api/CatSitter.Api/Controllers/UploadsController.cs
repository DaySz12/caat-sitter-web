using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatSitter.Api.Controllers;

/// <summary>
/// Replaces Supabase Storage: stores files on a local/mounted disk volume and serves them
/// back as static files under /uploads/**. See Program.cs for the static file mapping.
/// </summary>
[ApiController]
[Route("api/uploads")]
[Authorize]
public class UploadsController : ControllerBase
{
    private static readonly HashSet<string> AllowedBuckets = new()
    {
        "avatars", "cat-images", "sitter-documents", "checklist-photos", "chat-images", "review-photos",
    };

    private readonly IWebHostEnvironment _env;

    public UploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("{bucket}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string bucket, IFormFile file)
    {
        if (!AllowedBuckets.Contains(bucket))
            return BadRequest(new { message = "ไม่รู้จัก bucket นี้" });
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "ไม่พบไฟล์ที่อัปโหลด" });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
        var fileName = $"{Guid.NewGuid():N}{ext}";

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", bucket);
        Directory.CreateDirectory(uploadsRoot);

        var fullPath = Path.Combine(uploadsRoot, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var publicUrl = $"/uploads/{bucket}/{fileName}";
        return Ok(new { url = publicUrl });
    }
}
