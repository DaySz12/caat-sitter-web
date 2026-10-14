using CatSitter.Api.Dtos;
using CatSitter.Api.Services;
using CatSitter.Api.Services.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// Bookings where the signed-in user is either the owner or the sitter.
    [HttpGet("mine")]
    public async Task<ActionResult<List<BookingDto>>> GetMine([FromQuery] string? status)
    {
        var bookings = await _bookingService.GetMineAsync(User.GetUserId(), status);
        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id)
    {
        try
        {
            var booking = await _bookingService.GetByIdAsync(id, User.GetUserId(), User.GetRole());
            if (booking is null) return NotFound();
            return Ok(booking);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize(Roles = "owner")]
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest req)
    {
        try
        {
            var created = await _bookingService.CreateAsync(User.GetUserId(), req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingDto>> UpdateStatus(Guid id, UpdateBookingStatusRequest req)
    {
        try
        {
            var updated = await _bookingService.UpdateStatusAsync(id, User.GetUserId(), User.GetRole(), req);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/refund-request")]
    public async Task<ActionResult<BookingDto>> RequestRefund(Guid id, RequestRefundRequest req)
    {
        try
        {
            var updated = await _bookingService.RequestRefundAsync(id, User.GetUserId(), User.GetRole(), req);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/payment-slip")]
    public async Task<ActionResult<BookingDto>> UploadPaymentSlip(Guid id, UploadPaymentSlipRequest req)
    {
        try
        {
            var updated = await _bookingService.UploadPaymentSlipAsync(id, User.GetUserId(), User.GetRole(), req);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
