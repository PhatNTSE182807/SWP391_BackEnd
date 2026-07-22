using N_Tier.Application.Models;

namespace N_Tier.Application.Models.Keyword;

public class KeywordPagedRequest : PagedRequest
{
    public string SearchTerm { get; set; }
}
