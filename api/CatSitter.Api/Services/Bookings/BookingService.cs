using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using CatSitter.Api.Repositories;

namespace CatSitter.Api.Services.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;

    private static readonly string[] ValidNextStatuses =
        { "confirmed", "in_progress", "completed", "cancelled", "rejected" };

    private static readonly Dictionary<string, string> StatusChangeTexts = new()
    {
        ["confirmed"] = "ได้รับการยืนยันแล้ว",
        ["in_progress"] = "เริ่มการดูแลแล้ว",
        ["completed"] = "เสร็จสิ้นแล้ว",
        ["cancelled"] = "ถูกยกเลิก",
        ["rejected"] = "ถูกปฏิเสธ",
    };

    public BookingService(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<List<BookingDto>> GetMineAsync(Guid userId, string? status)
    {
        var bookings = await _bookingRepo.GetMineAsync(userId, status);
        var ids = bookings.Select(b => b.Id).ToList();
        var reviews = await _bookingRepo.GetReviewsForBookingsAsync(ids);
        return bookings.Select(b => b.ToDto(reviews.GetValueOrDefault(b.Id))).ToList();
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, Guid userId, string role)
    {
        var booking = await _bookingRepo.GetWithDetailsByIdAsync(id);
        if (booking is null) return null;
        if (booking.OwnerId != userId && booking.SitterId != userId && role != "admin")
        {
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์เข้าถึงข้อมูลการจองนี้");
        }

        var reviews = await _bookingRepo.GetReviewsForBookingsAsync(new List<Guid> { id });
        return booking.ToDto(reviews.GetValueOrDefault(id));
    }

    public async Task<BookingDto> CreateAsync(Guid ownerId, CreateBookingRequest req)
    {
        if (req.CatIds.Count == 0)
            throw new ArgumentException("กรุณาเลือกแมวอย่างน้อย 1 ตัว");
        if (req.EndDate < req.StartDate)
            throw new ArgumentException("วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มต้น");

        var sitterProfile = await _bookingRepo.GetSitterProfileAsync(req.SitterId);
        if (sitterProfile is null)
            throw new KeyNotFoundException("ไม่พบผู้รับฝากเลี้ยงรายนี้");

        var cats = await _bookingRepo.GetCatsByIdsAndOwnerAsync(req.CatIds, ownerId);
        if (cats.Count != req.CatIds.Count)
            throw new ArgumentException("พบข้อมูลแมวที่ไม่ถูกต้อง");

        var daysCount = req.EndDate.DayNumber - req.StartDate.DayNumber + 1;
        var basePrice = sitterProfile.PricePerDay * daysCount * cats.Count;
        var serviceFee = Math.Round(basePrice * 0.10m, 2);
        var totalPrice = basePrice + serviceFee;

        var owner = await _bookingRepo.GetUserAsync(ownerId);
        var firstCat = cats.FirstOrDefault();
        var serviceAddress = !string.IsNullOrWhiteSpace(req.ServiceAddress)
            ? req.ServiceAddress.Trim()
            : owner?.Address;
        var emergencyPhone = !string.IsNullOrWhiteSpace(req.EmergencyContactPhone)
            ? req.EmergencyContactPhone.Trim()
            : (!string.IsNullOrWhiteSpace(firstCat?.EmergencyContactPhone) ? firstCat.EmergencyContactPhone : owner?.SecondaryPhone);
        var vetClinic = string.IsNullOrWhiteSpace(req.PreferredVetClinic)
            ? firstCat?.EmergencyVetClinic
            : req.PreferredVetClinic.Trim();

        var booking = new Booking
        {
            BookingCode = $"CS-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
            OwnerId = ownerId,
            SitterId = req.SitterId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            DaysCount = daysCount,
            BasePrice = basePrice,
            ServiceFee = serviceFee,
            TotalPrice = totalPrice,
            SpecialInstructions = req.SpecialInstructions,
            ServiceAddress = serviceAddress,
            EmergencyContactPhone = emergencyPhone,
            PreferredVetClinic = vetClinic,
            Status = "pending",
            PaymentStatus = "unpaid",
        };
        booking.BookingCats = cats.Select(c => new BookingCat { BookingId = booking.Id, CatId = c.Id }).ToList();

        await _bookingRepo.AddAsync(booking);

        _bookingRepo.AddNotification(new Notification
        {
            UserId = req.SitterId,
            Title = "🐾 มีคำขอจองบริการดูแลแมวใหม่!",
            Body = "คุณได้รับคำขอจองบริการใหม่ โปรดตรวจสอบและตอบรับงาน",
            Type = "booking_created",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
        });

        await _bookingRepo.SaveChangesAsync();

        var created = await _bookingRepo.GetWithDetailsByIdAsync(booking.Id);
        return created!.ToDto();
    }

    public async Task<BookingDto> UpdateStatusAsync(Guid id, Guid userId, string role, UpdateBookingStatusRequest req)
    {
        if (!ValidNextStatuses.Contains(req.Status))
            throw new ArgumentException("สถานะไม่ถูกต้อง");

        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking is null)
            throw new KeyNotFoundException("ไม่พบข้อมูลการจอง");

        if (booking.OwnerId != userId && booking.SitterId != userId && role != "admin")
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์แก้ไขการจองนี้");

        var isSitterAction = req.Status is "confirmed" or "in_progress" or "completed" or "rejected";
        if (isSitterAction && booking.SitterId != userId && role != "admin")
            throw new UnauthorizedAccessException("เฉพาะพี่เลี้ยงเท่านั้นที่สามารถเปลี่ยนสถานะนี้ได้");

        if (req.Status == "cancelled" && booking.OwnerId != userId && role != "admin")
            throw new UnauthorizedAccessException("เฉพาะเจ้าของเท่านั้นที่สามารถยกเลิกได้");

        booking.Status = req.Status;
        if (req.CancellationReason is not null) booking.CancellationReason = req.CancellationReason;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        if (req.Status == "cancelled" && booking.PaymentStatus == "paid")
        {
            booking.RefundStatus = "requested";
            booking.RefundReason = req.CancellationReason ?? "ลูกค้ายกเลิกการจอง";
            booking.RefundRequestedAt = DateTimeOffset.UtcNow;

            var adminIds = await _bookingRepo.GetAdminUserIdsAsync();
            foreach (var aId in adminIds)
            {
                _bookingRepo.AddNotification(new Notification
                {
                    UserId = aId,
                    Title = "💸 มีคำขอยกเลิกและคืนเงิน (Refund)",
                    Body = $"การจอง {booking.BookingCode} ยอด {booking.TotalPrice:N2} บาท ถูกยกเลิกและมีคำขอคืนเงิน",
                    Type = "refund_requested",
                    DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
                });
            }
        }

        if (req.Status == "completed")
        {
            var openVisits = await _bookingRepo.GetOpenVisitsByBookingIdAsync(id);
            foreach (var visit in openVisits) visit.CheckOutTime = DateTimeOffset.UtcNow;

            var sitterUser = await _bookingRepo.GetUserAsync(booking.SitterId);
            if (sitterUser is not null)
            {
                sitterUser.WalletBalance += booking.BasePrice;
            }
        }

        var notifyUserId = userId == booking.OwnerId ? booking.SitterId : booking.OwnerId;
        var body = req.Status == "completed" && notifyUserId == booking.OwnerId
            ? $"การจอง {booking.BookingCode} เสร็จสิ้นแล้ว อย่าลืมให้คะแนนพี่เลี้ยงนะ ⭐"
            : $"การจอง {booking.BookingCode} {StatusChangeTexts[req.Status]}";

        _bookingRepo.AddNotification(new Notification
        {
            UserId = notifyUserId,
            Title = "อัปเดตสถานะการจอง",
            Body = body,
            Type = "booking_status_changed",
            DataJson = $"{{\"booking_id\":\"{booking.Id}\",\"status\":\"{req.Status}\"}}",
        });

        await _bookingRepo.SaveChangesAsync();

        var updated = await _bookingRepo.GetWithDetailsByIdAsync(id);
        var reviews = await _bookingRepo.GetReviewsForBookingsAsync(new List<Guid> { id });
        return updated!.ToDto(reviews.GetValueOrDefault(id));
    }

    public async Task<BookingDto> RequestRefundAsync(Guid id, Guid userId, string role, RequestRefundRequest req)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking is null)
            throw new KeyNotFoundException("ไม่พบข้อมูลการจอง");

        if (booking.OwnerId != userId && role != "admin")
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์ขอคืนเงินสำหรับการจองนี้");

        if (booking.PaymentStatus != "paid")
            throw new InvalidOperationException("ขอคืนเงินได้เฉพาะรายการที่ชำระเงินเรียบร้อยแล้วเท่านั้น");

        if (booking.Status == "completed")
            throw new InvalidOperationException("ไม่สามารถขอคืนเงินงานที่เสร็จสิ้นแล้วได้");

        booking.RefundStatus = "requested";
        booking.RefundReason = req.Reason.Trim();
        booking.RefundBankInfo = req.BankInfo.Trim();
        booking.RefundRequestedAt = DateTimeOffset.UtcNow;
        booking.Status = "cancelled";
        booking.CancellationReason = $"ขอยกเลิกและขอคืนเงิน: {req.Reason.Trim()}";
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        var adminIds = await _bookingRepo.GetAdminUserIdsAsync();
        foreach (var adminId in adminIds)
        {
            _bookingRepo.AddNotification(new Notification
            {
                UserId = adminId,
                Title = "💸 มีคำขอคืนเงิน (Refund Request)",
                Body = $"การจอง {booking.BookingCode} ยอด {booking.TotalPrice:N2} บาท ลูกค้าแจ้งขอคืนเงิน: {req.Reason.Trim()}",
                Type = "refund_requested",
                DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
            });
        }

        await _bookingRepo.SaveChangesAsync();

        var updated = await _bookingRepo.GetWithDetailsByIdAsync(id);
        var reviews = await _bookingRepo.GetReviewsForBookingsAsync(new List<Guid> { id });
        return updated!.ToDto(reviews.GetValueOrDefault(id));
    }

    public async Task<BookingDto> UploadPaymentSlipAsync(Guid id, Guid userId, string role, UploadPaymentSlipRequest req)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking is null)
            throw new KeyNotFoundException("ไม่พบข้อมูลการจอง");

        if (booking.OwnerId != userId && role != "admin")
            throw new UnauthorizedAccessException("คุณไม่มีสิทธิ์แนบสลิปสำหรับการจองนี้");

        if (string.IsNullOrWhiteSpace(req.SlipUrl))
            throw new ArgumentException("กรุณาระบุ URL สลิปการโอนเงิน");

        booking.PaymentSlipUrl = req.SlipUrl.Trim();
        booking.PaymentSlipUploadedAt = DateTimeOffset.UtcNow;
        booking.PaymentStatus = "pending_verification";
        booking.PaymentRejectionReason = null;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        var adminIds = await _bookingRepo.GetAdminUserIdsAsync();
        foreach (var adminId in adminIds)
        {
            _bookingRepo.AddNotification(new Notification
            {
                UserId = adminId,
                Title = "🧾 มีการแนบสลิปชำระเงินใหม่",
                Body = $"การจอง {booking.BookingCode} มียอดชำระ {booking.TotalPrice:N2} บาท รอการตรวจสอบสลิป",
                Type = "payment_slip_uploaded",
                DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
            });
        }

        await _bookingRepo.SaveChangesAsync();

        var updated = await _bookingRepo.GetWithDetailsByIdAsync(id);
        var reviews = await _bookingRepo.GetReviewsForBookingsAsync(new List<Guid> { id });
        return updated!.ToDto(reviews.GetValueOrDefault(id));
    }
}
