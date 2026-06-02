namespace N_Tier.Application.Models.Search;

public class ElasticsearchSettings
{
    public string Uri { get; set; }
    public string DefaultIndex { get; set; } = "papers";
}
