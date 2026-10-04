namespace CatSitter.Api.Services;

/// <summary>
/// Ports the "Smart Matching" scoring + GPS distance ranking that used to live in the
/// Flutter app's SitterRepository, so the web API returns the same ranked results.
/// </summary>
public static class MatchingService
{
    private const double EarthRadiusKm = 6371.0;

    public static double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    /// <summary>
    /// Weighted 0-100 score: rating 40%, GPS proximity 35%, review volume 15%, price 10%.
    /// </summary>
    public static double ComputeMatchScore(decimal ratingAvg, double? distanceKm, int reviewCount, decimal pricePerDay)
    {
        var ratingFactor = Math.Clamp((double)ratingAvg / 5.0, 0.0, 1.0);

        var proximityFactor = 0.7; // default when GPS unknown
        if (distanceKm is not null)
        {
            proximityFactor = Math.Max(0.0, 1.0 - (distanceKm.Value / 25.0));
        }

        var reviewFactor = Math.Clamp(Math.Min(reviewCount, 15) / 15.0, 0.0, 1.0);
        var priceFactor = Math.Clamp(1.0 - ((double)pricePerDay / 1000.0), 0.2, 1.0);

        var total = (ratingFactor * 0.40) + (proximityFactor * 0.35) + (reviewFactor * 0.15) + (priceFactor * 0.10);
        return Math.Round(total * 100);
    }

    /// <summary>
    /// Derives display-only "special skill" tags from the sitter's bio text (same heuristic
    /// as the mobile app) so the UI can show filterable badges without a dedicated column.
    /// </summary>
    public static List<string> ExtractSkillsFromBio(string? bio)
    {
        var text = (bio ?? string.Empty).ToLowerInvariant();
        var skills = new List<string>();

        if (text.Contains("ยา")) skills.Add("ป้อนยาได้");
        if (text.Contains("เด็ก") || text.Contains("ลูกแมว")) skills.Add("รับแมวเด็ก");
        if (text.Contains("ดุ") || text.Contains("กลัว") || text.Contains("พิเศษ")) skills.Add("ดูแลแมวขี้กลัว");
        if (text.Contains("ขน") || text.Contains("หวี")) skills.Add("แปรงขนทุกวัน");
        if (text.Contains("ห้องแอร์") || text.Contains("แอร์")) skills.Add("ห้องแอร์ 24 ชม.");

        if (skills.Count == 0)
        {
            skills.Add("ดูแลใกล้ชิด");
            skills.Add("ส่งรูปอัปเดตตลอด");
        }

        return skills;
    }
}
