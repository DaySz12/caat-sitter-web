using System.Text.Json;
using CatSitter.Api.Entities;
using CatSitter.Api.Services;

namespace CatSitter.Api.Dtos;

public static class Mapping
{
    public static UserDto ToDto(this User u) => new(
        u.Id, u.Email, u.Name, u.Phone, u.AvatarUrl, u.Role, u.Status, u.WalletBalance, u.CreatedAt,
        u.Address, u.BuildingNote, u.Latitude, u.Longitude, u.LineId, u.SecondaryPhone, u.EmergencyContactName
    );

    public static UserBriefDto ToBriefDto(this User u) => new(u.Id, u.Name, u.AvatarUrl, u.Role);

    public static CatDto ToDto(this Cat c) => new(
        c.Id, c.OwnerId, c.Name, c.Breed, c.AgeYears, c.AgeMonths, c.Weight, c.Gender,
        c.IsNeutered, c.PhotoUrl, c.DietaryHabits, c.MedicalNotes, c.EmergencyVetClinic,
        c.EmergencyContactPhone, c.Vaccines, c.CreatedAt
    );

    public static SitterDto ToDto(this SitterProfile s, double? distanceKm = null, double? matchScore = null) => new(
        s.Id, s.Bio, s.PricePerDay, s.Address, s.Latitude, s.Longitude, s.ExperienceYears,
        s.MaxCatsPerDay, s.ServicePhotos, s.IsVerified, s.IsAvailable, s.RatingAvg, s.ReviewCount,
        s.SocialLine, s.SocialFacebook, s.SocialInstagram,
        s.User?.ToDto(), distanceKm, matchScore, MatchingService.ExtractSkillsFromBio(s.Bio)
    );

    public static BookingDto ToDto(this Booking b, Review? review = null) => new(
        b.Id, b.BookingCode, b.OwnerId, b.SitterId, b.StartDate, b.EndDate, b.DaysCount,
        b.BasePrice, b.ServiceFee, b.TotalPrice, b.Status, b.PaymentStatus,
        b.PaymentSlipUrl, b.PaymentSlipUploadedAt, b.PaymentRejectionReason,
        b.SpecialInstructions, b.CancellationReason,
        b.ServiceAddress, b.EmergencyContactPhone, b.PreferredVetClinic,
        b.RefundStatus, b.RefundReason, b.RefundBankInfo,
        b.CreatedAt,
        b.Owner?.ToDto(), b.Sitter?.ToDto(),
        b.BookingCats?.Where(bc => bc.Cat is not null).Select(bc => bc.Cat!.ToDto()).ToList() ?? new List<CatDto>(),
        review?.ToDto()
    );

    public static ReviewDto ToDto(this Review r) => new(
        r.Id, r.BookingId, r.SitterId, r.Rating, r.Comment, r.Photos, r.CreatedAt, r.Reviewer?.ToBriefDto()
    );

    public static NotificationDto ToDto(this Notification n) => new(
        n.Id, n.Title, n.Body, n.Type, ReadBookingId(n.DataJson), n.IsRead, n.CreatedAt
    );

    public static MessageDto ToDto(this Message m) => new(
        m.Id, m.RoomId, m.SenderId, m.Content, m.ImageUrl, m.IsRead, m.CreatedAt
    );

    public static AttendanceDto ToDto(this AttendanceRecord a) => new(
        a.Id, a.CheckInTime, a.CheckOutTime, a.LocationLat, a.LocationLng, a.CheckinPhotoUrl, a.CheckoutPhotoUrl, a.Note
    );

    public static ChecklistDto ToDto(this Checklist c) => new(
        c.Id, c.Date, c.Title,
        c.Items.Select(i => new ChecklistItemDto(i.Id, i.TaskName, i.IsCompleted, i.PhotoUrl, i.CompletedAt, i.Note)).ToList()
    );

    public static BannerDto ToDto(this Banner b) => new(
        b.Id, b.Type, b.Title, b.ImageUrl, b.LinkUrl, b.IsActive, b.DisplayOrder, b.CreatedAt
    );

    public static AdminBannerDto ToAdminDto(this Banner b) => new(
        b.Id, b.Type, b.Title, b.ImageUrl, b.LinkUrl, b.IsActive, b.DisplayOrder, b.CreatedAt
    );

    // Notifications store their link target as JSON, e.g. {"booking_id":"..."}.
    private static Guid? ReadBookingId(string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(dataJson);
            return doc.RootElement.TryGetProperty("booking_id", out var value) && Guid.TryParse(value.GetString(), out var id)
                ? id
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
