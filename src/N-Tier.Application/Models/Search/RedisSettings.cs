namespace N_Tier.Application.Models.Search;

public class RedisSettings
{
    public string Configuration { get; set; }
    public int DefaultCacheDurationMinutes { get; set; } = 15;
}
