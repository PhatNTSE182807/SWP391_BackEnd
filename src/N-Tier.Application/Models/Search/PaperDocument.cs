using Nest;

namespace N_Tier.Application.Models.Search;


[ElasticsearchType(IdProperty = nameof(PaperId))]
public class PaperDocument
{
    [Keyword]
    public Guid PaperId { get; set; }

    [Text(Analyzer = "standard")]
    public string Title { get; set; }

    [Text(Analyzer = "standard")]
    public string Abstract { get; set; }

    [Keyword]
    public int? PublicationYear { get; set; }

    [Keyword]
    public string Language { get; set; }

    [Number]
    public int? CitedByCount { get; set; }

    [Boolean]
    public bool? IsOpenAccess { get; set; }

    [Keyword]
    public string Doi { get; set; }

    [Keyword]
    public string PaperType { get; set; }

    [Date]
    public DateTime CreatedAt { get; set; }

    [Date]
    public DateTime? UpdatedAt { get; set; }
}
