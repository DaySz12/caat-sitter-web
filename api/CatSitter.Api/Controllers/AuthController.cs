using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { message = "กรุณากรอกอีเมล รหัสผ่าน และชื่อให้ครบถ้วน" });
        }
        if (req.Password.Length < 6)
        {
            return BadRequest(new { message = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร" });
        }

        var email = req.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
        {
            return Conflict(new { message = "อีเมลนี้ถูกใช้งานแล้ว" });
        }

        var role = req.Role == "sitter" ? "sitter" : "owner";
        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Name = req.Name.Trim(),
            Phone = req.Phone,
            Role = role,
            Status = role == "sitter" ? "pending" : "approved",
        };
        _db.Users.Add(user);

        if (role == "sitter")
        {
            _db.SitterProfiles.Add(new SitterProfile
            {
                Id = user.Id,
                Bio = "สวัสดีครับ/ค่ะ ยินดีดูแลน้องแมวอย่างดีที่สุด",
                PricePerDay = 300.00m,
                IsVerified = false,
            });
        }

        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.ToDto()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "อีเมลหรือรหัสผ่านไม่ถูกต้อง" });
        }

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.ToDto()));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();
        return Ok(user.ToDto());
    }
}
