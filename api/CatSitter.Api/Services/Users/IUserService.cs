using CatSitter.Api.Dtos;

namespace CatSitter.Api.Services.Users;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserSummaryDto> GetMySummaryAsync(Guid userId);
    Task<UserDto> UpdateMeAsync(Guid userId, UpdateProfileRequest req);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req);
}
