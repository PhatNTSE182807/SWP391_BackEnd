namespace N_Tier.Application.Models.ApiSource;

public class CreateApiSourceModel
{
    public string SourceName { get; set; }
    public string BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
