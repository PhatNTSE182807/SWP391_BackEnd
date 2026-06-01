using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class DomainSourceMapping
{
    public Guid MappingId { get; set; }

    public Guid DomainId { get; set; }

    public Guid SourceId { get; set; }

    public string SourceRecordId { get; set; }

    public string SourceRecordUrl { get; set; }

    public string SourceSpecificData { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ResearchDomain Domain { get; set; }
}

