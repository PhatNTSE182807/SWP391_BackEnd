using System.Collections.Generic;

namespace N_Tier.Application.Models;

public class PagedResponse<T>
{
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public IEnumerable<T> Results { get; set; }

    public PagedResponse()
    {
    }

    public PagedResponse(IEnumerable<T> results, long total, int page, int size)
    {
        Results = results;
        Total = total;
        Page = page;
        Size = size;
    }
}
