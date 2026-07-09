using System;
using System.Collections.Generic;
using N_Tier.Application.Models.JournalSourceMapping;
using N_Tier.Application.Models.JournalTopic;
using N_Tier.Application.Models.JournalType;
using N_Tier.Application.Models.Paper;

namespace N_Tier.Application.Models.Journal;

public class JournalResponseModel
{
    public Guid JournalId { get; set; }
    public string JournalName { get; set; }
    public string IssnL { get; set; }
    public string Publisher { get; set; }
    public string HomepageUrl { get; set; }
    public bool IsOpenAccess { get; set; }
    public bool IsCore { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual JournalTypeResponseModel JournalTypeNavigation { get; set; }
    public virtual ICollection<JournalSourceMappingResponseModel> JournalSourceMappings { get; set; } = new List<JournalSourceMappingResponseModel>();
    public virtual ICollection<JournalTopicResponseModel> JournalTopics { get; set; } = new List<JournalTopicResponseModel>();
    public virtual ICollection<PaperResponseModel> Papers { get; set; } = new List<PaperResponseModel>();
}
