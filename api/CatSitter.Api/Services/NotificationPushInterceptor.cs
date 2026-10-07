using System.Runtime.CompilerServices;
using CatSitter.Api.Dtos;
using CatSitter.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CatSitter.Api.Services;

/// <summary>
/// Pushes every saved Notification to its user as a live "notification" event, so the many
/// places that add notifications (bookings, admin, care, reviews) don't each have to.
/// </summary>
public class NotificationPushInterceptor : SaveChangesInterceptor
{
    private readonly LiveEvents _events;
    private readonly ConditionalWeakTable<DbContext, List<Notification>> _pending = new();

    public NotificationPushInterceptor(LiveEvents events)
    {
        _events = events;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Publish(eventData.Context);
        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Publish(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null) _pending.Remove(eventData.Context);
    }

    // Added entities turn Unchanged once saved, so remember them before the save.
    private void Capture(DbContext? context)
    {
        if (context is null) return;
        var added = context.ChangeTracker.Entries<Notification>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();
        if (added.Count > 0) _pending.AddOrUpdate(context, added);
    }

    private void Publish(DbContext? context)
    {
        if (context is null || !_pending.TryGetValue(context, out var saved)) return;
        _pending.Remove(context);
        foreach (var notification in saved) _events.Publish(notification.UserId, "notification", notification.ToDto());
    }
}
