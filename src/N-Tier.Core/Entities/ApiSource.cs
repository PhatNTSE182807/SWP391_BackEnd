using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class ApiSource
{
    public Guid SourceId { get; set; }

    public string SourceName { get; set; }

    public string BaseUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AuthorSourceMapping> AuthorSourceMappings { get; set; } = new List<AuthorSourceMapping>();

    public virtual ICollection<JournalSourceMapping> JournalSourceMappings { get; set; } = new List<JournalSourceMapping>();

    public virtual ICollection<KeywordSourceMapping> KeywordSourceMappings { get; set; } = new List<KeywordSourceMapping>();

    public virtual ICollection<PaperKeyword> PaperKeywords { get; set; } = new List<PaperKeyword>();

    public virtual ICollection<PaperSourceMapping> PaperSourceMappings { get; set; } = new List<PaperSourceMapping>();

    public virtual ICollection<PipelineRun> PipelineRuns { get; set; } = new List<PipelineRun>();

    public virtual ICollection<Work> Works { get; set; } = new List<Work>();
}
