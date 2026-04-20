namespace Backend.Models;

public record DataRequestInfo
{
    public Guid RequestId { get; init; }
    public string ClientId { get; init; } = default!;
    public RequestState State { get; set; }
    public string? ErrorMessage { get; set; }
    public DataPayloadDto? Payload { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
