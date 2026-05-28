using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class AuthorSourceMapping
{
    public Guid MappingId { get; set; }

    public Guid AuthorId { get; set; }

    public Guid SourceId { get; set; }

    public string SourceRecordId { get; set; }

    public string SourceRecordUrl { get; set; }

    public string SourceSpecificData { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Author Author { get; set; }

    public virtual ApiSource Source { get; set; }
}
