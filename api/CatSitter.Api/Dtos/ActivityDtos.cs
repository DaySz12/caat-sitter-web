namespace CatSitter.Api.Dtos;

// DTOs for the features layered on top of bookings: notifications, reviews, chat and the
// daily care log (check-in/out + checklists).

public record UserBriefDto(Guid Id, string Name, string? AvatarUrl, string Role);

public record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string Type,
    Guid? BookingId,
    bool IsRead,
    DateTimeOffset CreatedAt
);

/// Counts behind the sidebar badges and the dashboard stat tiles.
public record UserSummaryDto(
    int UnreadNotifications,
    int UnreadMessages,
    int PendingRequests,
    int ActiveBookings,
    int CompletedBookings,
    decimal TotalEarnings,
    decimal MonthEarnings,
    decimal TotalSpent
);

public record ReviewDto(
    Guid Id,
    Guid BookingId,
    Guid SitterId,
    int Rating,
    string? Comment,
    List<string> Photos,
    DateTimeOffset CreatedAt,
    UserBriefDto? Reviewer
);

public record CreateReviewRequest(Guid BookingId, int Rating, string? Comment, List<string>? Photos);

public record ChatRoomDto(
    Guid Id,
    UserBriefDto Other,
    Guid? BookingId,
    string LastMessage,
    DateTimeOffset LastMessageTime,
    int UnreadCount
);

public record MessageDto(
    Guid Id,
    Guid RoomId,
    Guid SenderId,
    string? Content,
    string? ImageUrl,
    bool IsRead,
    DateTimeOffset CreatedAt
);

public record StartChatRequest(Guid UserId, Guid? BookingId);

public record SendMessageRequest(string? Content, string? ImageUrl);

public record AttendanceDto(
    Guid Id,
    DateTimeOffset CheckInTime,
    DateTimeOffset? CheckOutTime,
    double? LocationLat,
    double? LocationLng,
    string? CheckinPhotoUrl,
    string? CheckoutPhotoUrl,
    string? Note
);

public record ChecklistItemDto(
    Guid Id,
    string TaskName,
    bool IsCompleted,
    string? PhotoUrl,
    DateTimeOffset? CompletedAt,
    string? Note
);

public record ChecklistDto(Guid Id, DateOnly Date, string Title, List<ChecklistItemDto> Items);

public record BookingCareDto(List<AttendanceDto> Attendance, List<ChecklistDto> Checklists);

public record CheckInRequest(double? Lat, double? Lng, string? PhotoUrl, string? Note);

public record CheckOutRequest(string? PhotoUrl, string? Note);

public record UpdateChecklistItemRequest(bool? IsCompleted, string? PhotoUrl, string? Note);
