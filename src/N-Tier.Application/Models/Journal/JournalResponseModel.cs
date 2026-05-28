using System;
using N_Tier.Application.Models;

namespace N_Tier.Application.Models.Journal;

public class JournalResponseModel : BaseResponseModel
{
    public Guid JournalId { get; set; }
    public string JournalName { get; set; }
    public string IssnL { get; set; }
    public string Publisher { get; set; }
    public string HomepageUrl { get; set; }
    public bool IsOpenAccess { get; set; }
    public bool IsCore { get; set; }
    public DateTime CreatedAt { get; set; }
}
