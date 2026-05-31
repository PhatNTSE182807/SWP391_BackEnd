using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class ResearchDomain
{
    public Guid DomainId { get; set; }

    public string DomainName { get; set; }

    public string NormalizedName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DomainSourceMapping> DomainSourceMappings { get; set; } = new List<DomainSourceMapping>();

    public virtual ICollection<ResearchField> ResearchFields { get; set; } = new List<ResearchField>();
}

