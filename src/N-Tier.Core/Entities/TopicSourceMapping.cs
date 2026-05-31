using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class TopicSourceMapping
{
    public Guid MappingId { get; set; }

    public Guid TopicId { get; set; }

    public Guid SourceId { get; set; }

    public string SourceRecordId { get; set; }

    public string SourceRecordUrl { get; set; }

    public string SourceSpecificData { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ResearchTopic Topic { get; set; }
}

