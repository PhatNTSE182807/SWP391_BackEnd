using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class PaperTopic
{
    public Guid PaperTopicId { get; set; }

    public Guid PaperId { get; set; }

    public Guid TopicId { get; set; }

    public decimal? Score { get; set; }

    public Guid SourceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Paper Paper { get; set; }

    public virtual ResearchTopic Topic { get; set; }
}

