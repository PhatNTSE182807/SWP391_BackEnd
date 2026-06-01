using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class PipelineRun
{
    public Guid RunId { get; set; }

    public Guid SourceId { get; set; }

    public string SourceEntity { get; set; }

    public string QueryKeyword { get; set; }

    public string Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public int RecordsFetched { get; set; }

    public int RecordsInserted { get; set; }

    public int RecordsFailed { get; set; }

    public string ErrorMessage { get; set; }

    public virtual ApiSource Source { get; set; }

    public virtual ICollection<Work> Works { get; set; } = new List<Work>();
}

