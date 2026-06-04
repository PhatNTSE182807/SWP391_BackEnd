using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class JournalType
{
    public Guid JournalTypeId { get; set; }

    public string TypeCode { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Journal> Journals { get; set; } = new List<Journal>();
}

