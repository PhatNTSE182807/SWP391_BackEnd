using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class Keyword
{
    public Guid KeywordId { get; set; }

    public string KeywordName { get; set; }

    public string NormalizedName { get; set; }

    public int? WorksCount { get; set; }

    public int? CitedByCount { get; set; }

    public string WorksApiUrl { get; set; }

    public DateOnly? SourceCreatedDate { get; set; }

    public DateTime? SourceUpdatedDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<KeywordSourceMapping> KeywordSourceMappings { get; set; } = new List<KeywordSourceMapping>();

    public virtual ICollection<PaperKeyword> PaperKeywords { get; set; } = new List<PaperKeyword>();
}

