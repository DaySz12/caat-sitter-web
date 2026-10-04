using CatSitter.Api.Entities;
using CatSitter.Api.Services;

namespace CatSitter.Api.Dtos;

public static class Mapping
{
    public static UserDto ToDto(this User u) => new(
        u.Id, u.Email, u.Name, u.Phone, u.AvatarUrl, u.Role, u.Status, u.WalletBalance, u.CreatedAt
    );

    public static CatDto ToDto(this Cat c) => new(
        c.Id, c.OwnerId, c.Name, c.Breed, c.AgeYears, c.AgeMonths, c.Weight, c.Gender,
        c.IsNeutered, c.PhotoUrl, c.DietaryHabits, c.MedicalNotes, c.Vaccines, c.CreatedAt
    );

    public static SitterDto ToDto(this SitterProfile s, double? distanceKm = null, double? matchScore = null) => new(
        s.Id, s.Bio, s.PricePerDay, s.Address, s.Latitude, s.Longitude, s.ExperienceYears,
        s.MaxCatsPerDay, s.ServicePhotos, s.IsVerified, s.IsAvailable, s.RatingAvg, s.ReviewCount,
        s.SocialLine, s.SocialFacebook, s.SocialInstagram,
        s.User?.ToDto(), distanceKm, matchScore, MatchingService.ExtractSkillsFromBio(s.Bio)
    );

    public static BookingDto ToDto(this Booking b) => new(
        b.Id, b.BookingCode, b.OwnerId, b.SitterId, b.StartDate, b.EndDate, b.DaysCount,
        b.BasePrice, b.ServiceFee, b.TotalPrice, b.Status, b.PaymentStatus,
        b.SpecialInstructions, b.CancellationReason, b.CreatedAt,
        b.Owner?.ToDto(), b.Sitter?.ToDto(),
        b.BookingCats?.Where(bc => bc.Cat is not null).Select(bc => bc.Cat!.ToDto()).ToList() ?? new List<CatDto>()
    );
}
