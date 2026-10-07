using CatSitter.Api.Entities;

namespace CatSitter.Api.Services;

/// The default task list a sitter ticks off each day of a booking.
public static class DailyChecklist
{
    private static readonly string[] BaseTasks =
    {
        "ให้อาหารและเปลี่ยนน้ำสะอาด",
        "ทำความสะอาดกระบะทราย",
        "เล่นและใช้เวลากับน้อง",
        "สังเกตการกิน การขับถ่าย และอาการทั่วไป",
        "ถ่ายรูปอัปเดตให้เจ้าของ",
    };

    private const string HealthTask = "ดูแลตามบันทึกสุขภาพ (ยา / อาหารพิเศษ)";

    public static Checklist Create(Guid bookingId, DateOnly date, bool hasHealthNotes, DateTimeOffset? createdAt = null)
    {
        var now = createdAt ?? DateTimeOffset.UtcNow;
        var tasks = hasHealthNotes ? BaseTasks.Append(HealthTask) : BaseTasks;
        var checklist = new Checklist { BookingId = bookingId, Date = date, Title = "งานดูแลประจำวัน", CreatedAt = now };

        // There is no position column, so items get UUIDv7 ids with increasing timestamps:
        // ordering by id then returns them in this list's order.
        checklist.Items = tasks
            .Select((task, i) => new ChecklistItem
            {
                Id = Guid.CreateVersion7(now.AddMilliseconds(i)),
                ChecklistId = checklist.Id,
                TaskName = task,
            })
            .ToList();

        return checklist;
    }
}
