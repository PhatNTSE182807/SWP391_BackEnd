using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class ResearchSubfield
{
    public Guid SubfieldId { get; set; }

    public Guid? FieldId { get; set; }

    public string SubfieldName { get; set; }

    public string NormalizedName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ResearchField Field { get; set; }

    public virtual ICollection<ResearchTopic> ResearchTopics { get; set; } = new List<ResearchTopic>();

    public virtual ICollection<SubfieldSourceMapping> SubfieldSourceMappings { get; set; } = new List<SubfieldSourceMapping>();
}

