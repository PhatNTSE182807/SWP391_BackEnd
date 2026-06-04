using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class PaperSourceMapping
{
    public Guid MappingId { get; set; }

    public Guid PaperId { get; set; }

    public Guid SourceId { get; set; }

    public Guid? RawWorkId { get; set; }

    public string SourceRecordId { get; set; }

    public string SourceRecordUrl { get; set; }

    public string SourceSpecificData { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Paper Paper { get; set; }
}

