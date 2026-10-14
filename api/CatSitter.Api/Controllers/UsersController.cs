using CatSitter.Api.Dtos;
using CatSitter.Api.Services;
using CatSitter.Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    /// Badge counts and dashboard stats for the signed-in user, polled by the web app's sidebar.
    [HttpGet("me/summary")]
    public async Task<ActionResult<UserSummaryDto>> GetMySummary()
    {
        var summary = await _userService.GetMySummaryAsync(User.GetUserId());
        return Ok(summary);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequest req)
    {
        try
        {
            var updated = await _userService.UpdateMeAsync(User.GetUserId(), req);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        try
        {
            await _userService.ChangePasswordAsync(User.GetUserId(), req);
            return Ok(new { message = "เปลี่ยนรหัสผ่านสำเร็จเรียบร้อยแล้ว" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
