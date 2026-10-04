using CatSitter.Api.Data;
using CatSitter.Api.Entities;

namespace CatSitter.Api.Services;

// Creates the test accounts (admin / owner / sitter, password "password123") on a fresh
// database so a new machine is usable right after `docker compose up`.
// Runs only while the users table is empty; disable with Seed__DemoData=false.
public static class DemoSeeder
{
    private const string DemoPassword = "password123";

    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

        var admin = new User { Email = "admin@test.com", PasswordHash = hash, Name = "Admin Test", Role = "admin" };
        var owner = new User { Email = "owner@test.com", PasswordHash = hash, Name = "Owner Test", Role = "owner" };
        var sitter = new User { Email = "sitter@test.com", PasswordHash = hash, Name = "Sitter Test", Role = "sitter" };
        sitter.SitterProfile = new SitterProfile
        {
            Id = sitter.Id,
            Bio = "ยินดีดูแลน้องแมวอย่างดีที่สุด ป้อนยาได้ รับแมวเด็ก",
            Address = "กรุงเทพมหานคร",
            IsVerified = true,
        };

        db.Users.AddRange(admin, owner, sitter);
        db.SaveChanges();
    }
}
