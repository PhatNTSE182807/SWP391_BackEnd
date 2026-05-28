namespace N_Tier.Application.Models.Journal;

public class UpdateJournalModel
{
    public string JournalName { get; set; }
    public string IssnL { get; set; }
    public string Publisher { get; set; }
    public string HomepageUrl { get; set; }
    public bool IsOpenAccess { get; set; }
    public bool IsCore { get; set; }
}
