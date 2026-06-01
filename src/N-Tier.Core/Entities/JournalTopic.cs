using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class JournalTopic
{
    public Guid JournalTopicId { get; set; }

    public Guid JournalId { get; set; }

    public Guid TopicId { get; set; }

    public int? WorksCount { get; set; }

    public decimal? TopicShare { get; set; }

    public Guid SourceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Journal Journal { get; set; }

    public virtual ResearchTopic Topic { get; set; }
}

