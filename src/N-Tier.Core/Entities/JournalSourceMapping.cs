using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class JournalSourceMapping
{
    public Guid MappingId { get; set; }

    public Guid JournalId { get; set; }

    public Guid SourceId { get; set; }

    public Guid? RawSourceId { get; set; }

    public string SourceRecordId { get; set; }

    public string SourceRecordUrl { get; set; }

    public string SourceSpecificData { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Journal Journal { get; set; }
}

