using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class ResearchTopic
{
    public Guid TopicId { get; set; }

    public Guid? SubfieldId { get; set; }

    public string TopicName { get; set; }

    public string NormalizedName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<JournalTopic> JournalTopics { get; set; } = new List<JournalTopic>();

    public virtual ICollection<PaperTopic> PaperTopics { get; set; } = new List<PaperTopic>();

    public virtual ResearchSubfield Subfield { get; set; }

    public virtual ICollection<TopicSourceMapping> TopicSourceMappings { get; set; } = new List<TopicSourceMapping>();
}

