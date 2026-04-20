using Backend.Models;

namespace Backend.Interfaces;

public interface IDataRequestService
{
    Task<Guid> CreateOrGetRequestAsync(string clientId, CancellationToken cancellationToken);
    DataRequestStatusResponse? GetStatus(Guid requestId);
    DataRequestInfo? GetRequest(Guid requestId);
    void MarkProcessing(Guid requestId);
    void MarkCompleted(Guid requestId, DataPayloadDto payload);
    void MarkFailed(Guid requestId, string errorMessage);
}
