using CatSitter.Api.Dtos;

namespace CatSitter.Api.Services.Cats;

public interface ICatService
{
    Task<List<CatDto>> GetMineAsync(Guid ownerId);
    Task<CatDto?> GetByIdAsync(Guid id);
    Task<CatDto> CreateAsync(Guid ownerId, CatUpsertRequest req);
    Task<CatDto> UpdateAsync(Guid id, Guid ownerId, CatUpsertRequest req);
    Task DeleteAsync(Guid id, Guid ownerId);
}
