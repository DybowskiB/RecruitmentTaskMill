using Backend.Interfaces;
using Backend.Models;
using Backend.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class DataRequestService(
    IMemoryCache memoryCache,
    IRequestQueue requestQueue,
    IOptions<DataGenerationOptions> options) : IDataRequestService
{
    private readonly DataGenerationOptions _options = options.Value;

    private readonly Dictionary<Guid, DataRequestInfo> _requests = [];
    private readonly Lock _lock = new();

    public async Task<Guid> CreateOrGetRequestAsync(string clientId, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(clientId);

        if (memoryCache.TryGetValue<DataPayloadDto>(cacheKey, out var cachedPayload) && cachedPayload is not null)
        {
            var completedRequest = new DataRequestInfo
            {
                RequestId = Guid.NewGuid(),
                ClientId = clientId,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                State = RequestState.Completed,
                Payload = cachedPayload
            };

            lock (_lock)
            {
                _requests[completedRequest.RequestId] = completedRequest;
            }

            return completedRequest.RequestId;
        }

        Guid? requestIdToEnqueue = null;

        lock (_lock)
        {
            var existingInProgress = _requests.Values.FirstOrDefault(r => r.ClientId == clientId && (r.State == RequestState.Pending || r.State == RequestState.Processing));

            if (existingInProgress is not null)
            {
                return existingInProgress.RequestId;
            }

            var request = new DataRequestInfo
            {
                RequestId = Guid.NewGuid(),
                ClientId = clientId,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                State = RequestState.Pending
            };

            _requests[request.RequestId] = request;
            requestIdToEnqueue = request.RequestId;
        }

        // Note: consider adding retry and error handling for enqueue failures.
        await requestQueue.EnqueueAsync(requestIdToEnqueue.Value, cancellationToken);

        return requestIdToEnqueue.Value;
    }

    public DataRequestStatusResponse? GetStatus(Guid requestId)
    {
        lock (_lock)
        {
            if (!_requests.TryGetValue(requestId, out var request))
            {
                return null;
            }

            return new DataRequestStatusResponse
            {
                RequestId = request.RequestId,
                State = request.State,
                ErrorMessage = request.ErrorMessage,
                Payload = request.Payload
            };
        }
    }

    public DataRequestInfo? GetRequest(Guid requestId)
    {
        lock (_lock)
        {
            _requests.TryGetValue(requestId, out var request);
            return request;
        }
    }

    public void MarkProcessing(Guid requestId)
    {
        lock (_lock)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                request.State = RequestState.Processing;
            }
        }
    }

    public void MarkCompleted(Guid requestId, DataPayloadDto payload)
    {
        lock (_lock)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                request.State = RequestState.Completed;
                request.Payload = payload;

                memoryCache.Set(GetCacheKey(request.ClientId), payload, TimeSpan.FromMinutes(_options.CacheMinutes));
            }
        }
    }

    public void MarkFailed(Guid requestId, string errorMessage)
    {
        lock (_lock)
        {
            if (_requests.TryGetValue(requestId, out var request))
            {
                request.State = RequestState.Failed;
                request.ErrorMessage = errorMessage;
            }
        }
    }

    private static string GetCacheKey(string clientId) => $"data:{clientId}";
}