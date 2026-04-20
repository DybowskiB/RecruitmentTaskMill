using Backend.Models;

namespace Backend.Interfaces;

public interface IDataGenerationService
{
    Task<DataPayloadDto> GenerateAsync(string clientId, CancellationToken cancellationToken);
}
