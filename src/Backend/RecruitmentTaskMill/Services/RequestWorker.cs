using Backend.Interfaces;

namespace Backend.Services;

public sealed class RequestWorker(
    IRequestQueue requestQueue,
    IDataRequestService dataRequestService,
    IDataGenerationService dataGenerationService,
    ILogger<RequestWorker> logger) : BackgroundService
{
    private readonly IRequestQueue _requestQueue = requestQueue;
    private readonly IDataRequestService _dataRequestService = dataRequestService;
    private readonly IDataGenerationService _dataGenerationService = dataGenerationService;
    private readonly ILogger<RequestWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid requestId;

            try
            {
                requestId = await _requestQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var request = _dataRequestService.GetRequest(requestId);
            if (request is null)
            {
                continue;
            }

            try
            {
                _dataRequestService.MarkProcessing(requestId);

                var payload = await _dataGenerationService.GenerateAsync(request.ClientId, stoppingToken);

                _dataRequestService.MarkCompleted(requestId, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process request {RequestId}", requestId);
                _dataRequestService.MarkFailed(requestId, ex.Message);
            }
        }
    }
}
