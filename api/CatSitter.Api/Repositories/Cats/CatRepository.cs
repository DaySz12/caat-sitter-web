using CatSitter.Api.Data;
using CatSitter.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Repositories.Cats;

public class CatRepository : ICatRepository
{
    private readonly AppDbContext _db;

    public CatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Cat>> GetMineAsync(Guid ownerId)
    {
        return await _db.Cats
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Cat?> GetByIdAsync(Guid id)
    {
        return await _db.Cats.FindAsync(id);
    }

    public async Task AddAsync(Cat cat)
    {
        await _db.Cats.AddAsync(cat);
    }

    public void Remove(Cat cat)
    {
        _db.Cats.Remove(cat);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
