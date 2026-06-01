using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class ResearchField
{
    public Guid FieldId { get; set; }

    public Guid? DomainId { get; set; }

    public string FieldName { get; set; }

    public string NormalizedName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ResearchDomain Domain { get; set; }

    public virtual ICollection<FieldSourceMapping> FieldSourceMappings { get; set; } = new List<FieldSourceMapping>();

    public virtual ICollection<ResearchSubfield> ResearchSubfields { get; set; } = new List<ResearchSubfield>();
}

