using Backend.Interfaces;
using Backend.Models;
using Backend.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services;

public class DataGenerationService(IOptions<DataGenerationOptions> options) : IDataGenerationService
{
    private readonly int errorFrequency = 10;
    private readonly DataGenerationOptions _options = options.Value;

    private int _requestCounter = 0;

    public async Task<DataPayloadDto> GenerateAsync(string clientId, CancellationToken cancellationToken)
    {
        var currentRequestNumber = Interlocked.Increment(ref _requestCounter);

        await Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), cancellationToken);

        if (currentRequestNumber % errorFrequency == 0)
        {
            throw new InvalidOperationException($"Simulated backend error on every {errorFrequency}th request.");
        }

        var generatedNumber = CreateNumber(clientId);

        var now = DateTimeOffset.UtcNow;

        return new DataPayloadDto
        {
            ClientId = clientId,
            Message = $"Data generated for client {clientId[..8]}",
            GeneratedNumber = generatedNumber,
            GeneratedAtUtc = now,
            CachedUntilUtc = now.AddMinutes(_options.CacheMinutes)
        };
    }

    private static int CreateNumber(string clientId)
    {
        var input = $"{clientId}:{DateTimeOffset.UtcNow:yyyyMMddHHmm}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToInt32(bytes, 0) & int.MaxValue;
    }
}