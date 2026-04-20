namespace Backend.Interfaces;

public interface IRequestQueue
{
    ValueTask EnqueueAsync(Guid requestId, CancellationToken cancellationToken);
    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
