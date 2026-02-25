namespace ECommerce.Api.Settings;

public class RateLimitSettings
{
    public const string SectionName = "RateLimiting";

    public int MaxRequests { get; set; } = 100;
    public int WindowMinutes { get; set; } = 1;
}
