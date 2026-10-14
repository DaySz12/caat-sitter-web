using CatSitter.Api.Dtos;
using CatSitter.Api.Services;
using CatSitter.Api.Services.Cats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/cats")]
[Authorize]
public class CatsController : ControllerBase
{
    private readonly ICatService _catService;

    public CatsController(ICatService catService)
    {
        _catService = catService;
    }

    /// Cats belonging to the signed-in owner.
    [HttpGet("mine")]
    public async Task<ActionResult<List<CatDto>>> GetMine()
    {
        var cats = await _catService.GetMineAsync(User.GetUserId());
        return Ok(cats);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatDto>> GetById(Guid id)
    {
        var cat = await _catService.GetByIdAsync(id);
        if (cat is null) return NotFound();
        return Ok(cat);
    }

    [HttpPost]
    public async Task<ActionResult<CatDto>> Create(CatUpsertRequest req)
    {
        try
        {
            var created = await _catService.CreateAsync(User.GetUserId(), req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CatDto>> Update(Guid id, CatUpsertRequest req)
    {
        try
        {
            var updated = await _catService.UpdateAsync(id, User.GetUserId(), req);
            return Ok(updated);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _catService.DeleteAsync(id, User.GetUserId());
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
