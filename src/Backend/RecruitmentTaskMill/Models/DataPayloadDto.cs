namespace Backend.Models;

public record DataPayloadDto
{
    public string ClientId { get; set; } = default!;
    public string Message { get; set; } = default!;
    public int GeneratedNumber { get; set; }
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public DateTimeOffset CachedUntilUtc { get; set; }
}
