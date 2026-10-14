using CatSitter.Api.Entities;

namespace CatSitter.Api.Repositories.Cats;

public interface ICatRepository
{
    Task<List<Cat>> GetMineAsync(Guid ownerId);
    Task<Cat?> GetByIdAsync(Guid id);
    Task AddAsync(Cat cat);
    void Remove(Cat cat);
    Task SaveChangesAsync();
}
