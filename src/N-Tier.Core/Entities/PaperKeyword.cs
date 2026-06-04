using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class PaperKeyword
{
    public Guid PaperKeywordId { get; set; }

    public Guid PaperId { get; set; }

    public Guid KeywordId { get; set; }

    public decimal? Score { get; set; }

    public Guid SourceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Keyword Keyword { get; set; }

    public virtual Paper Paper { get; set; }
}

