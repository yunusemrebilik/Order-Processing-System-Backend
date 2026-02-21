namespace ECommerce.Application.Common.Settings;

public class CacheSettings
{
    public const string SectionName = "Cache";

    public int ProductTtlInDays { get; set; } = 1;
    public int CartTtlInDays { get; set; } = 7;
}
