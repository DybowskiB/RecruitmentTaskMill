namespace Backend.Models;

public record DataRequestStatusResponse
{
    public Guid RequestId { get; set; }
    public RequestState State { get; set; }
    public string? ErrorMessage { get; set; }
    public DataPayloadDto? Payload { get; set; }
}
