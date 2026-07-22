using System;
using System.Collections.Generic;
using N_Tier.Application.Models.PaperAuthor;

namespace N_Tier.Application.Models.Author
{
    public class AuthorResponseModel
    {
        public Guid AuthorId { get; set; }
        public string DisplayName { get; set; }
        public string NormalizedName { get; set; }
        public string FullName { get; set; }
        public string Orcid { get; set; }
        public int? WorksCount { get; set; }
        public int? CitedByCount { get; set; }
        public int? HIndex { get; set; }
        public int? I10Index { get; set; }
        public double? TwoYearMeanCitedness { get; set; }
        public string RawAuthorNames { get; set; }
        public string DisplayNameAlternatives { get; set; }
        public string Affiliations { get; set; }
        public string LastKnownInstitutions { get; set; }
        public string Topics { get; set; }
        public string TopicShare { get; set; }
        public string XConcepts { get; set; }
        public string CountsByYear { get; set; }
        public string WorksApiUrl { get; set; }
        public DateTime? SourceCreatedDate { get; set; }
        public DateTime? SourceUpdatedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<PaperAuthorResponseModel> PaperAuthors { get; set; } = new List<PaperAuthorResponseModel>();
    }
}
