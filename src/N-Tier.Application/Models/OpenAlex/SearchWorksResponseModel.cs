using System.Collections.Generic;

namespace N_Tier.Application.Models.OpenAlex;

public class SearchWorksResponseModel
{
    public MetaData Meta { get; set; }
    public List<WorkItemModel> Results { get; set; } = new List<WorkItemModel>();
}

public class MetaData
{
    public int Count { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }
}

public class WorkItemModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Doi { get; set; }
    public int? PublicationYear { get; set; }
    public string PublicationDate { get; set; }
    public List<string> Authors { get; set; } = new List<string>();
    public string JournalName { get; set; }
}
