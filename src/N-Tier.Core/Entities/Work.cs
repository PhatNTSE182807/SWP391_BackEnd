using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class Work
{
    public Guid RawWorkId { get; set; }

    public Guid SourceId { get; set; }

    public string SourceEntity { get; set; }

    public string SourceRecordId { get; set; }

    public string QueryKeyword { get; set; }

    public Guid PipelineRunId { get; set; }

    public string RawData { get; set; }

    public DateTime FetchedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public string ProcessedStatus { get; set; }

    public string ProcessError { get; set; }

    public virtual ICollection<PaperSourceMapping> PaperSourceMappings { get; set; } = new List<PaperSourceMapping>();

    public virtual PipelineRun PipelineRun { get; set; }

    public virtual ApiSource Source { get; set; }
}
