using System;
using N_Tier.Application.Models.JournalType;

namespace N_Tier.Application.Models.Journal;

public class JournalResponseModel
{
    public Guid JournalId { get; set; }
    public string JournalName { get; set; }
    public bool IsOpenAccess { get; set; }
    public bool IsCore { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual JournalTypeResponseModel JournalTypeNavigation { get; set; }
}
