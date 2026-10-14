using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;

namespace CatSitter.Api.Repositories.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<UserSummaryDto> GetUserSummaryAsync(Guid userId);
    Task SaveChangesAsync();
}
