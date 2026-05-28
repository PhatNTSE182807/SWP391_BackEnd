using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class Journal
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

    public string HomepageUrl { get; set; }

    public string CountryCode { get; set; }

    public int? WorksCount { get; set; }

    public int? CitedByCount { get; set; }

    public int? OaWorksCount { get; set; }

    public int? HIndex { get; set; }

    public int? I10Index { get; set; }

    public decimal? TwoYearMeanCitedness { get; set; }

    public bool? IsOpenAccess { get; set; }

    public bool? IsInDoaj { get; set; }

    public bool? IsCore { get; set; }

    public int? FirstPublicationYear { get; set; }

    public int? LastPublicationYear { get; set; }

    public string Subjects { get; set; }

    public string Topics { get; set; }

    public string CountsByYear { get; set; }

    public DateTime? SourceCreatedDate { get; set; }

    public DateTime? SourceUpdatedDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<JournalSourceMapping> JournalSourceMappings { get; set; } = new List<JournalSourceMapping>();

    public virtual ICollection<Paper> Papers { get; set; } = new List<Paper>();
}
