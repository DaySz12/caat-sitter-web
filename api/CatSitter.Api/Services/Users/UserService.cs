using CatSitter.Api.Dtos;
using CatSitter.Api.Repositories.Users;

namespace CatSitter.Api.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user?.ToDto();
    }

    public async Task<UserSummaryDto> GetMySummaryAsync(Guid userId)
    {
        return await _userRepo.GetUserSummaryAsync(userId);
    }

    public async Task<UserDto> UpdateMeAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("ไม่พบผู้ใช้งาน");

        if (req.Name is not null) user.Name = req.Name.Trim();
        if (req.Phone is not null) user.Phone = req.Phone.Trim();
        if (req.AvatarUrl is not null) user.AvatarUrl = req.AvatarUrl;
        if (req.Address is not null) user.Address = req.Address.Trim();
        if (req.BuildingNote is not null) user.BuildingNote = req.BuildingNote.Trim();
        if (req.Latitude is not null) user.Latitude = req.Latitude;
        if (req.Longitude is not null) user.Longitude = req.Longitude;
        if (req.LineId is not null) user.LineId = req.LineId.Trim();
        if (req.SecondaryPhone is not null) user.SecondaryPhone = req.SecondaryPhone.Trim();
        if (req.EmergencyContactName is not null) user.EmergencyContactName = req.EmergencyContactName.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userRepo.SaveChangesAsync();
        return user.ToDto();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
        {
            throw new ArgumentException("รหัสผ่านใหม่ต้องมีความยาวอย่างน้อย 6 ตัวอักษร");
        }

        var user = await _userRepo.GetByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("ไม่พบผู้ใช้งาน");

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("รหัสผ่านปัจจุบันไม่ถูกต้อง");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _userRepo.SaveChangesAsync();
    }
}
