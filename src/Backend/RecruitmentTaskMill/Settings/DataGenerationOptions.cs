namespace Backend.Settings;

public record DataGenerationOptions
{
    public int DelaySeconds { get; set; } = 60;
    public int CacheMinutes { get; set; } = 5;
}
