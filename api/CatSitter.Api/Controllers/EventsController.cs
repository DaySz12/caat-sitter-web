using System.Threading.Channels;
using CatSitter.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatSitter.Api.Controllers;

/// <summary>
/// Server-Sent Events stream of the signed-in user's live events. Browsers' EventSource can't
/// send headers, so the JWT comes in the ?access_token= query string (see Program.cs).
/// </summary>
[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(20);

    private readonly LiveEvents _events;
    private readonly IHostApplicationLifetime _lifetime;

    public EventsController(LiveEvents events, IHostApplicationLifetime lifetime)
    {
        _events = events;
        _lifetime = lifetime;
    }

    [HttpGet]
    public async Task Stream()
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no"; // don't let a reverse proxy buffer the stream

        // End the stream on client disconnect or app shutdown, so `docker compose stop` isn't held up.
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted, _lifetime.ApplicationStopping);
        var reader = _events.Subscribe(User.GetUserId(), out var unsubscribe);
        try
        {
            await Response.WriteAsync("retry: 5000\n\n", stop.Token);
            await Response.Body.FlushAsync(stop.Token);

            while (!stop.IsCancellationRequested)
            {
                await WriteNextAsync(reader, stop.Token);
                await Response.Body.FlushAsync(stop.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Client went away or the app is stopping.
        }
        finally
        {
            unsubscribe();
        }
    }

    // Writes the next event, or a comment line after a quiet spell so proxies keep the connection open.
    private async Task WriteNextAsync(ChannelReader<LiveEvent> reader, CancellationToken stop)
    {
        using var wait = CancellationTokenSource.CreateLinkedTokenSource(stop);
        wait.CancelAfter(KeepAliveInterval);
        try
        {
            var evt = await reader.ReadAsync(wait.Token);
            await Response.WriteAsync($"event: {evt.Type}\ndata: {evt.Json}\n\n", stop);
        }
        catch (OperationCanceledException) when (!stop.IsCancellationRequested)
        {
            await Response.WriteAsync(": keep-alive\n\n", stop);
        }
    }
}
