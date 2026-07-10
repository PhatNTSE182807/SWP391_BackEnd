using System;
using System.Collections.Generic;
using N_Tier.Application.Models.JournalTopic;
using N_Tier.Application.Models.JournalType;
using N_Tier.Application.Models.Paper;

namespace N_Tier.Application.Models.Journal;

public class JournalResponseModel
{
    public Guid JournalId { get; set; }
    public string JournalName { get; set; }
    public string NormalizedName { get; set; }
    public string IssnL { get; set; }
    public string IssnPrint { get; set; }
    public string IssnElectronic { get; set; }
    public string Publisher { get; set; }
    public string HostOrganizationName { get; set; }
    public string JournalType { get; set; }
    public Guid? JournalTypeId { get; set; }
    public string HomepageUrl { get; set; }
    public string CountryCode { get; set; }
    public int? WorksCount { get; set; }
    public int? CitedByCount { get; set; }
    public int? OaWorksCount { get; set; }
    public int? HIndex { get; set; }
    public int? I10Index { get; set; }
    public double? TwoYearMeanCitedness { get; set; }
    public bool? IsOpenAccess { get; set; }
    public bool? IsInDoaj { get; set; }
    public bool? IsCore { get; set; }
    public int? FirstPublicationYear { get; set; }
    public int? LastPublicationYear { get; set; }
    public string CountsByYear { get; set; }
    public DateTime? SourceCreatedDate { get; set; }
    public DateTime? SourceUpdatedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual JournalTypeResponseModel JournalTypeNavigation { get; set; }
    public virtual ICollection<JournalTopicResponseModel> JournalTopics { get; set; } = new List<JournalTopicResponseModel>();
    public virtual ICollection<PaperResponseModel> Papers { get; set; } = new List<PaperResponseModel>();
}
