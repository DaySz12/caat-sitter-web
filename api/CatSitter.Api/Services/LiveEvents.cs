using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace CatSitter.Api.Services;

public record LiveEvent(string Type, string Json);

/// <summary>
/// In-memory fan-out of real-time events ("notification", "message", "care") to each user's
/// open event streams (see EventsController). A user can have several streams (tabs/devices).
/// Single-instance only: with more than one API instance this would need a shared backplane.
/// </summary>
public class LiveEvents
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Channel<LiveEvent>, byte>> _streams = new();

    public ChannelReader<LiveEvent> Subscribe(Guid userId, out Action unsubscribe)
    {
        var channel = Channel.CreateUnbounded<LiveEvent>(new UnboundedChannelOptions { SingleReader = true });
        var userStreams = _streams.GetOrAdd(userId, _ => new ConcurrentDictionary<Channel<LiveEvent>, byte>());
        userStreams[channel] = 0;

        unsubscribe = () =>
        {
            userStreams.TryRemove(channel, out _);
            channel.Writer.TryComplete();
        };
        return channel.Reader;
    }

    public void Publish(Guid userId, string type, object data)
    {
        if (!_streams.TryGetValue(userId, out var userStreams) || userStreams.IsEmpty) return;

        var evt = new LiveEvent(type, JsonSerializer.Serialize(data, JsonOptions));
        foreach (var channel in userStreams.Keys) channel.Writer.TryWrite(evt);
    }
}
