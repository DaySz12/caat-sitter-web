using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

/// One chat room per owner/sitter pair. New messages are pushed to both people as a live
/// "message" event (EventsController); the web app also polls slowly as a fallback.
[ApiController]
[Route("api/chat")]
[Authorize(Roles = "owner,sitter")]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly LiveEvents _events;

    public ChatController(AppDbContext db, LiveEvents events)
    {
        _db = db;
        _events = events;
    }

    [HttpGet("rooms")]
    public async Task<ActionResult<List<ChatRoomDto>>> GetRooms()
    {
        var userId = User.GetUserId();
        var rooms = await _db.ChatRooms
            .Where(r => r.OwnerId == userId || r.SitterId == userId)
            .OrderByDescending(r => r.LastMessageTime)
            .ToListAsync();

        var otherIds = rooms.Select(r => r.OwnerId == userId ? r.SitterId : r.OwnerId).Distinct().ToList();
        var others = await _db.Users.Where(u => otherIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

        var roomIds = rooms.Select(r => r.Id).ToList();
        var unread = await _db.Messages
            .Where(m => roomIds.Contains(m.RoomId) && m.SenderId != userId && !m.IsRead)
            .GroupBy(m => m.RoomId)
            .Select(g => new { RoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count);

        var result = rooms
            .Where(r => others.ContainsKey(r.OwnerId == userId ? r.SitterId : r.OwnerId))
            .Select(r => new ChatRoomDto(
                r.Id,
                others[r.OwnerId == userId ? r.SitterId : r.OwnerId].ToBriefDto(),
                r.BookingId,
                r.LastMessage,
                r.LastMessageTime,
                unread.GetValueOrDefault(r.Id)
            ))
            .ToList();

        return Ok(result);
    }

    /// Opens (or returns the existing) room with another user. Owners may message any sitter,
    /// e.g. to ask questions before booking; sitters may only message owners who booked them.
    [HttpPost("rooms")]
    public async Task<ActionResult<ChatRoomDto>> StartRoom(StartChatRequest req)
    {
        var userId = User.GetUserId();
        var other = await _db.Users.FindAsync(req.UserId);
        if (other is null || other.Id == userId) return NotFound(new { message = "ไม่พบผู้ใช้นี้" });

        Guid ownerId, sitterId;
        if (User.GetRole() == "owner" && other.Role == "sitter")
        {
            (ownerId, sitterId) = (userId, other.Id);
        }
        else if (User.GetRole() == "sitter" && other.Role == "owner")
        {
            (ownerId, sitterId) = (other.Id, userId);
            if (!await _db.Bookings.AnyAsync(b => b.OwnerId == ownerId && b.SitterId == sitterId))
                return BadRequest(new { message = "เริ่มแชทได้เฉพาะกับเจ้าของแมวที่เคยจองบริการคุณ" });
        }
        else
        {
            return BadRequest(new { message = "แชทได้เฉพาะระหว่างเจ้าของแมวและผู้รับฝากเลี้ยง" });
        }

        var room = await _db.ChatRooms.FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.SitterId == sitterId);
        if (room is null)
        {
            room = new ChatRoom { OwnerId = ownerId, SitterId = sitterId, BookingId = req.BookingId };
            _db.ChatRooms.Add(room);
            await _db.SaveChangesAsync();
        }

        var unread = await _db.Messages.CountAsync(m => m.RoomId == room.Id && m.SenderId != userId && !m.IsRead);
        return Ok(new ChatRoomDto(room.Id, other.ToBriefDto(), room.BookingId, room.LastMessage, room.LastMessageTime, unread));
    }

    /// Messages in a room, oldest first. Pass `after` to fetch only newer ones while polling.
    /// Fetching marks the other person's messages as read.
    [HttpGet("rooms/{id:guid}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(Guid id, [FromQuery] DateTimeOffset? after)
    {
        var userId = User.GetUserId();
        var room = await FindRoomForUser(id, userId);
        if (room is null) return NotFound();

        var query = _db.Messages.Where(m => m.RoomId == id);
        if (after is { } since) query = query.Where(m => m.CreatedAt > since.ToUniversalTime());

        var messages = await query.OrderByDescending(m => m.CreatedAt).Take(200).ToListAsync();
        messages.Reverse();

        await _db.Messages
            .Where(m => m.RoomId == id && m.SenderId != userId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

        return Ok(messages.Select(m => m.ToDto()).ToList());
    }

    [HttpPost("rooms/{id:guid}/messages")]
    public async Task<ActionResult<MessageDto>> SendMessage(Guid id, SendMessageRequest req)
    {
        var content = req.Content?.Trim();
        if (string.IsNullOrEmpty(content) && string.IsNullOrEmpty(req.ImageUrl))
            return BadRequest(new { message = "กรุณาพิมพ์ข้อความหรือแนบรูปภาพ" });

        var userId = User.GetUserId();
        var room = await FindRoomForUser(id, userId);
        if (room is null) return NotFound();

        var message = new Message
        {
            RoomId = id,
            SenderId = userId,
            Content = string.IsNullOrEmpty(content) ? null : content,
            ImageUrl = req.ImageUrl,
        };
        _db.Messages.Add(message);

        room.LastMessage = message.Content ?? "📷 รูปภาพ";
        room.LastMessageTime = message.CreatedAt;
        room.UpdatedAt = message.CreatedAt;

        await _db.SaveChangesAsync();

        var dto = message.ToDto();
        _events.Publish(room.OwnerId, "message", dto);
        _events.Publish(room.SitterId, "message", dto);
        return Ok(dto);
    }

    private Task<ChatRoom?> FindRoomForUser(Guid roomId, Guid userId) =>
        _db.ChatRooms.FirstOrDefaultAsync(r => r.Id == roomId && (r.OwnerId == userId || r.SitterId == userId));
}
