using Backend.Interfaces;
using System.Threading.Channels;

namespace Backend.Services;

public class RequestQueue : IRequestQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid requestId, CancellationToken cancellationToken)
        => _channel.Writer.WriteAsync(requestId, cancellationToken);

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAsync(cancellationToken);
}
