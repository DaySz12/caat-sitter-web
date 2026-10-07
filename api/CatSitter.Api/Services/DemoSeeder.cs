using CatSitter.Api.Data;
using CatSitter.Api.Entities;

namespace CatSitter.Api.Services;

// Creates the test accounts (admin / owner / sitter, password "password123") on a fresh
// database so a new machine is usable right after `docker compose up`, then fills them with
// sample cats, bookings in every status, reviews, chats, notifications and a care log so every
// page has something to show. Disable with Seed__DemoData=false.
public static class DemoSeeder
{
    private const string DemoPassword = "password123";

    public static void Seed(AppDbContext db)
    {
        SeedAccounts(db);
        SeedDemoContent(db);
    }

    // Runs only while the users table is empty.
    private static void SeedAccounts(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

        var admin = new User { Email = "admin@test.com", PasswordHash = hash, Name = "Admin Test", Role = "admin" };
        var owner = new User { Email = "owner@test.com", PasswordHash = hash, Name = "Owner Test", Role = "owner" };
        var sitter = new User { Email = "sitter@test.com", PasswordHash = hash, Name = "Sitter Test", Role = "sitter" };
        sitter.SitterProfile = new SitterProfile
        {
            Id = sitter.Id,
            Bio = "ยินดีดูแลน้องแมวอย่างดีที่สุด ป้อนยาได้ รับแมวเด็ก",
            Address = "กรุงเทพมหานคร",
            IsVerified = true,
        };

        db.Users.AddRange(admin, owner, sitter);
        db.SaveChanges();
    }

    // Runs once — only while the demo owner and demo sitter have no cats or bookings — and only
    // creates or touches @test.com demo accounts, never real ones.
    private static void SeedDemoContent(AppDbContext db)
    {
        var owner = db.Users.FirstOrDefault(u => u.Email == "owner@test.com");
        var sitter = db.Users.FirstOrDefault(u => u.Email == "sitter@test.com");
        if (owner is null || sitter is null) return;
        if (db.Cats.Any(c => c.OwnerId == owner.Id) ||
            db.Bookings.Any(b => b.OwnerId == owner.Id || b.SitterId == sitter.Id)) return;

        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);
        var today = BangkokTime.Today;

        var sitterProfile = db.SitterProfiles.Find(sitter.Id);
        if (sitterProfile is not null && sitterProfile.Latitude is null)
        {
            sitterProfile.Latitude = 13.7563;
            sitterProfile.Longitude = 100.5018;
            sitterProfile.ExperienceYears = 4;
        }

        var owner2 = GetOrCreateUser(db, "owner2@test.com", "น้ำหวาน รักแมว", "owner", hash);
        var mali = GetOrCreateSitter(db, "sitter2@test.com", "มะลิ ใจดี", hash, new SitterProfile
        {
            Bio = "เลี้ยงแมวมา 8 ปี ป้อนยาได้ หวีขนให้ทุกวัน มีห้องแอร์ให้น้องพักผ่อน",
            PricePerDay = 450m, Address = "อารีย์ กรุงเทพฯ", Latitude = 13.7797, Longitude = 100.5446,
            ExperienceYears = 8, MaxCatsPerDay = 4, IsVerified = true,
        });
        var tonkla = GetOrCreateSitter(db, "sitter3@test.com", "ต้นกล้า สายเหมียว", hash, new SitterProfile
        {
            Bio = "รับลูกแมวและแมวเด็ก ดูแลใกล้ชิด ส่งรูปและวิดีโออัปเดตทุกวัน",
            PricePerDay = 300m, Address = "บางนา กรุงเทพฯ", Latitude = 13.6688, Longitude = 100.6050,
            ExperienceYears = 3, MaxCatsPerDay = 3, IsVerified = true,
        });
        var puifai = GetOrCreateSitter(db, "sitter4@test.com", "ปุยฝ้าย แคทแคร์", hash, new SitterProfile
        {
            Bio = "ถนัดดูแลแมวขี้กลัวและแมวที่ต้องการการดูแลพิเศษ มีห้องแอร์ 24 ชม. แยกให้น้องแมว",
            PricePerDay = 550m, Address = "ทองหล่อ กรุงเทพฯ", Latitude = 13.7325, Longitude = 100.5784,
            ExperienceYears = 5, MaxCatsPerDay = 2, IsVerified = true,
        });

        var makham = AddCat(db, owner, "มะขาม", "สก็อตติช โฟลด์", 2, 3, 4.2m, "male",
            "อาหารเม็ดวันละ 2 มื้อ (เช้า-เย็น) และขนมแมวเลียตอนบ่าย", null, new() { "รวม 4 โรค", "พิษสุนัขบ้า" });
        var somo = AddCat(db, owner, "ส้มโอ", "แมวไทยสีส้ม", 1, 2, 3.6m, "female",
            "อาหารเปียกมื้อเย็น ชอบกินน้ำจากน้ำพุแมว", null, new() { "รวม 4 โรค" });
        var khaopan = AddCat(db, owner, "ข้าวปั้น", "เปอร์เซีย", 6, 0, 5.1m, "male",
            "อาหารสูตรโรคไตเท่านั้น", "กินยาบำรุงไตวันละ 1 เม็ดหลังอาหารเช้า แพ้อาหารทะเล",
            new() { "รวม 4 โรค", "พิษสุนัขบ้า", "ลิวคีเมีย" });
        var mochi = AddCat(db, owner2, "โมจิ", "บริติช ช็อตแฮร์", 3, 0, 4.8m, "female",
            "อาหารเม็ดเช้า-เย็น", null, new() { "รวม 4 โรค", "พิษสุนัขบ้า" });

        // Demo owner's bookings — one in every status.
        var active = AddBooking(db, owner, sitter, new() { makham, somo }, today.AddDays(-1), today.AddDays(2), "in_progress",
            instructions: "มะขามชอบให้ลูบคาง ส้มโอขี้อ้อนมาก ฝากเล่นไม้ตกแมวด้วยนะคะ");
        var request = AddBooking(db, owner, sitter, new() { khaopan }, today.AddDays(10), today.AddDays(12), "pending",
            instructions: "ข้าวปั้นต้องกินยาหลังอาหารเช้าทุกวันค่ะ", createdOn: today.AddDays(-1));
        var pastWithSitter = AddBooking(db, owner, sitter, new() { makham }, today.AddDays(-30), today.AddDays(-27), "completed");
        var toReview = AddBooking(db, owner, mali, new() { somo, khaopan }, today.AddDays(-10), today.AddDays(-8), "completed");
        var upcoming = AddBooking(db, owner, mali, new() { makham, somo }, today.AddDays(5), today.AddDays(7), "confirmed");
        AddBooking(db, owner, tonkla, new() { somo }, today.AddDays(-20), today.AddDays(-19), "cancelled",
            cancellationReason: "เปลี่ยนแผนการเดินทาง");
        var pastWithPuifai = AddBooking(db, owner, puifai, new() { khaopan }, today.AddDays(-45), today.AddDays(-43), "completed");

        // A second owner, so sitters have several reviews and a busier schedule.
        var c1 = AddBooking(db, owner2, sitter, new() { mochi }, today.AddDays(-60), today.AddDays(-58), "completed");
        var c2 = AddBooking(db, owner2, mali, new() { mochi }, today.AddDays(-50), today.AddDays(-47), "completed");
        var c3 = AddBooking(db, owner2, tonkla, new() { mochi }, today.AddDays(-40), today.AddDays(-39), "completed");
        var c4 = AddBooking(db, owner2, puifai, new() { mochi }, today.AddDays(-25), today.AddDays(-23), "completed");
        var c5 = AddBooking(db, owner2, mali, new() { mochi }, today.AddDays(-15), today.AddDays(-14), "completed");
        var c6 = AddBooking(db, owner2, sitter, new() { mochi }, today.AddDays(-4), today.AddDays(-2), "completed");
        AddBooking(db, owner2, sitter, new() { mochi }, today.AddDays(3), today.AddDays(4), "confirmed");

        AddReview(db, pastWithSitter, 5, "ดูแลดีมากค่ะ ส่งรูปมาให้ดูทุกวัน มะขามดูมีความสุขมาก");
        AddReview(db, pastWithPuifai, 4, "ใส่ใจเรื่องยาของข้าวปั้นดีมาก แต่ตอบแชทช้านิดนึงค่ะ");
        AddReview(db, c1, 5, "พี่เลี้ยงใจดีมาก โมจิไม่เครียดเลย");
        AddReview(db, c2, 5, "มืออาชีพมากค่ะ หวีขนให้ทุกวัน ขนโมจิสวยกว่าตอนอยู่บ้านอีก 😂");
        AddReview(db, c3, 4, "ดูแลดี ส่งรูปบ่อย ประทับใจค่ะ");
        AddReview(db, c4, 5, "ห้องแอร์เย็นสบาย น้องขี้กลัวแต่พี่เลี้ยงใจเย็นมาก");
        AddReview(db, c5, 5, "จองเป็นครั้งที่สองแล้ว ไว้ใจได้เลยค่ะ");
        AddReview(db, c6, 5, "อัปเดตทุกวัน น่ารักมากค่ะ แนะนำเลย");

        SeedCareLog(db, active, today);

        AddChatRoom(db, owner, sitter, active.Id, new()
        {
            (owner, BangkokTime.At(today.AddDays(-2), 19, 0), "สวัสดีค่ะ ฝากมะขามกับส้มโอด้วยนะคะ 🙏", true),
            (sitter, BangkokTime.At(today.AddDays(-2), 19, 5), "ยินดีครับ เดี๋ยวส่งรูปอัปเดตให้ทุกรอบที่เข้าไปดูแลเลยครับ", true),
            (sitter, BangkokTime.At(today.AddDays(-1), 10, 35), "วันนี้น้องๆ กินข้าวหมดเลยครับ ส้มโอขอเล่นไม้ตกแมวไม่หยุดเลย 😂", true),
            (owner, BangkokTime.At(today.AddDays(-1), 12, 0), "ขอบคุณมากค่ะ มะขามกินยากนิดนึง ถ้าไม่กินลองผสมขนมแมวเลียนะคะ", true),
            (sitter, NotAfterNow(BangkokTime.At(today, 9, 10)), "เช้านี้มะขามกินหมดชามเลยครับ ไม่ต้องผสมขนม 👍", false),
        });
        AddChatRoom(db, owner, mali, upcoming.Id, new()
        {
            (owner, BangkokTime.At(today.AddDays(-6), 14, 0), "สวัสดีค่ะ รับแมว 2 ตัวช่วงต้นสัปดาห์หน้าได้ไหมคะ", true),
            (mali, BangkokTime.At(today.AddDays(-6), 14, 20), "ได้เลยค่ะ ยินดีดูแลค่ะ 😊", true),
            (owner, BangkokTime.At(today.AddDays(-6), 14, 25), "ขอบคุณค่ะ เดี๋ยวกดจองเลยนะคะ", true),
        });
        AddChatRoom(db, owner2, sitter, c6.Id, new()
        {
            (owner2, BangkokTime.At(today.AddDays(-1), 20, 0), "ขอบคุณที่ดูแลโมจินะคะ ไว้จะจองอีกค่ะ", false),
        });

        AddNotification(db, owner, "📍 พี่เลี้ยงเช็กอินแล้ว", $"พี่เลี้ยงมาถึงและเริ่มดูแลน้องแมวแล้ว (การจอง {active.BookingCode})",
            "care_check_in", active, NotAfterNow(BangkokTime.At(today, 8, 45)), isRead: false);
        AddNotification(db, owner, "🏠 พี่เลี้ยงเช็กเอาต์แล้ว", $"ดูสรุปงานดูแลรอบนี้ได้ในหน้ารายละเอียดการจอง {active.BookingCode}",
            "care_check_out", active, BangkokTime.At(today.AddDays(-1), 10, 30), isRead: true);
        AddNotification(db, owner, "อัปเดตสถานะการจอง", $"การจอง {upcoming.BookingCode} ได้รับการยืนยันแล้ว",
            "booking_status_changed", upcoming, BangkokTime.At(today.AddDays(-5), 9, 0), isRead: false);
        AddNotification(db, owner, "อัปเดตสถานะการจอง", $"การจอง {toReview.BookingCode} เสร็จสิ้นแล้ว อย่าลืมให้คะแนนพี่เลี้ยงนะ ⭐",
            "booking_status_changed", toReview, BangkokTime.At(today.AddDays(-8), 18, 0), isRead: false);
        AddNotification(db, sitter, "🐾 มีคำขอจองบริการดูแลแมวใหม่!", "คุณได้รับคำขอจองบริการใหม่ โปรดตรวจสอบและตอบรับงาน",
            "booking_created", request, BangkokTime.At(today.AddDays(-1), 11, 0), isRead: false);
        AddNotification(db, sitter, "⭐ คุณได้รับรีวิว 5 ดาว", "อัปเดตทุกวัน น่ารักมากค่ะ แนะนำเลย",
            "review_received", c6, BangkokTime.At(today.AddDays(-1), 20, 30), isRead: false);
        AddNotification(db, sitter, "⭐ คุณได้รับรีวิว 5 ดาว", "ดูแลดีมากค่ะ ส่งรูปมาให้ดูทุกวัน มะขามดูมีความสุขมาก",
            "review_received", pastWithSitter, BangkokTime.At(today.AddDays(-26), 20, 0), isRead: true);

        db.SaveChanges();

        // Ratings are derived from the reviews, exactly as ReviewsController keeps them.
        foreach (var sitterId in new[] { sitter.Id, mali.Id, tonkla.Id, puifai.Id })
        {
            var profile = db.SitterProfiles.Find(sitterId);
            var ratings = db.Reviews.Where(r => r.SitterId == sitterId).Select(r => r.Rating).ToList();
            if (profile is null || ratings.Count == 0) continue;
            profile.RatingAvg = Math.Round((decimal)ratings.Average(), 2);
            profile.ReviewCount = ratings.Count;
        }
        db.SaveChanges();
    }

    // Yesterday: two finished visits and a fully ticked checklist. Today: one open visit and a
    // half-done checklist, so the sitter can try check-out / ticking and the owner sees progress.
    private static void SeedCareLog(AppDbContext db, Booking booking, DateOnly today)
    {
        var yesterday = today.AddDays(-1);
        db.AttendanceRecords.AddRange(
            new AttendanceRecord
            {
                BookingId = booking.Id, SitterId = booking.SitterId,
                CheckInTime = BangkokTime.At(yesterday, 9, 0), CheckOutTime = BangkokTime.At(yesterday, 10, 30),
                Note = "น้องๆ กินข้าวหมด เล่นด้วยกันประมาณ 30 นาที", CreatedAt = BangkokTime.At(yesterday, 9, 0),
            },
            new AttendanceRecord
            {
                BookingId = booking.Id, SitterId = booking.SitterId,
                CheckInTime = BangkokTime.At(yesterday, 18, 0), CheckOutTime = BangkokTime.At(yesterday, 18, 45),
                Note = "ให้อาหารมื้อเย็นและเก็บกระบะทรายแล้ว", CreatedAt = BangkokTime.At(yesterday, 18, 0),
            },
            new AttendanceRecord
            {
                BookingId = booking.Id, SitterId = booking.SitterId,
                CheckInTime = NotAfterNow(BangkokTime.At(today, 8, 45)), CreatedAt = NotAfterNow(BangkokTime.At(today, 8, 45)),
            });

        var done = DailyChecklist.Create(booking.Id, yesterday, hasHealthNotes: false, BangkokTime.At(yesterday, 9, 0));
        var completedAt = BangkokTime.At(yesterday, 9, 5);
        foreach (var item in done.Items)
        {
            item.IsCompleted = true;
            item.CompletedAt = completedAt;
            completedAt = completedAt.AddMinutes(15);
        }

        var inProgress = DailyChecklist.Create(booking.Id, today, hasHealthNotes: false, NotAfterNow(BangkokTime.At(today, 8, 45)));
        foreach (var item in inProgress.Items.Take(2))
        {
            item.IsCompleted = true;
            item.CompletedAt = NotAfterNow(BangkokTime.At(today, 9, 0));
        }

        db.Checklists.AddRange(done, inProgress);
    }

    private static User GetOrCreateUser(AppDbContext db, string email, string name, string role, string hash)
    {
        var existing = db.Users.FirstOrDefault(u => u.Email == email);
        if (existing is not null) return existing;

        var user = new User { Email = email, PasswordHash = hash, Name = name, Role = role };
        db.Users.Add(user);
        return user;
    }

    private static User GetOrCreateSitter(AppDbContext db, string email, string name, string hash, SitterProfile profile)
    {
        var existing = db.Users.FirstOrDefault(u => u.Email == email);
        if (existing is not null) return existing;

        var user = new User { Email = email, PasswordHash = hash, Name = name, Role = "sitter" };
        profile.Id = user.Id;
        user.SitterProfile = profile;
        db.Users.Add(user);
        return user;
    }

    private static Cat AddCat(AppDbContext db, User owner, string name, string breed, int years, int months, decimal weight,
        string gender, string diet, string? medical, List<string> vaccines)
    {
        var cat = new Cat
        {
            OwnerId = owner.Id, Name = name, Breed = breed, AgeYears = years, AgeMonths = months, Weight = weight,
            Gender = gender, IsNeutered = true, DietaryHabits = diet, MedicalNotes = medical, Vaccines = vaccines,
        };
        db.Cats.Add(cat);
        return cat;
    }

    private static Booking AddBooking(AppDbContext db, User owner, User sitter, List<Cat> cats, DateOnly start, DateOnly end,
        string status, string? instructions = null, string? cancellationReason = null, DateOnly? createdOn = null)
    {
        var pricePerDay = db.SitterProfiles.Find(sitter.Id)?.PricePerDay ?? 300m;
        var days = end.DayNumber - start.DayNumber + 1;
        var basePrice = pricePerDay * days * cats.Count;
        var serviceFee = Math.Round(basePrice * 0.10m, 2);

        var created = createdOn ?? start.AddDays(-5);
        if (created > BangkokTime.Today) created = BangkokTime.Today;
        var createdAt = NotAfterNow(BangkokTime.At(created, 10, 0));

        var booking = new Booking
        {
            BookingCode = $"CS-{created:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
            OwnerId = owner.Id,
            SitterId = sitter.Id,
            StartDate = start,
            EndDate = end,
            DaysCount = days,
            BasePrice = basePrice,
            ServiceFee = serviceFee,
            TotalPrice = basePrice + serviceFee,
            Status = status,
            PaymentStatus = status == "completed" ? "paid" : "unpaid",
            SpecialInstructions = instructions,
            CancellationReason = cancellationReason,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        booking.BookingCats = cats.Select(c => new BookingCat { BookingId = booking.Id, CatId = c.Id }).ToList();
        db.Bookings.Add(booking);
        return booking;
    }

    private static void AddReview(AppDbContext db, Booking booking, int rating, string comment)
    {
        db.Reviews.Add(new Review
        {
            BookingId = booking.Id,
            ReviewerId = booking.OwnerId,
            SitterId = booking.SitterId,
            Rating = rating,
            Comment = comment,
            CreatedAt = BangkokTime.At(booking.EndDate.AddDays(1), 20, 0),
        });
    }

    private static void AddChatRoom(AppDbContext db, User owner, User sitter, Guid bookingId,
        List<(User Sender, DateTimeOffset At, string Text, bool IsRead)> messages)
    {
        var last = messages[^1];
        var room = new ChatRoom
        {
            OwnerId = owner.Id, SitterId = sitter.Id, BookingId = bookingId,
            LastMessage = last.Text, LastMessageTime = last.At, UpdatedAt = last.At,
        };
        room.Messages = messages
            .Select(m => new Message { RoomId = room.Id, SenderId = m.Sender.Id, Content = m.Text, IsRead = m.IsRead, CreatedAt = m.At })
            .ToList();
        db.ChatRooms.Add(room);
    }

    private static void AddNotification(AppDbContext db, User user, string title, string body, string type, Booking booking,
        DateTimeOffset createdAt, bool isRead)
    {
        db.Notifications.Add(new Notification
        {
            UserId = user.Id, Title = title, Body = body, Type = type,
            DataJson = $"{{\"booking_id\":\"{booking.Id}\"}}",
            IsRead = isRead, CreatedAt = createdAt,
        });
    }

    // Keeps "today at 09:10"-style sample times from landing in the future when seeding early in the morning.
    private static DateTimeOffset NotAfterNow(DateTimeOffset at)
    {
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        return at > now ? now : at;
    }
}
