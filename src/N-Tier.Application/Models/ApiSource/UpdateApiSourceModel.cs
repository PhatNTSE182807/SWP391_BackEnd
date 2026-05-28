namespace N_Tier.Application.Models.ApiSource;

public class UpdateApiSourceModel
{
    public string SourceName { get; set; }
    public string BaseUrl { get; set; }
    public bool IsActive { get; set; }
}
