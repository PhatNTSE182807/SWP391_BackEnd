

using System;
using System.Collections.Generic;
using N_Tier.Application.Models.AuthorSourceMapping;
using N_Tier.Application.Models.PaperAuthor;

namespace N_Tier.Application.Models.Author
{
    public class AuthorResponseModel
    {
        public Guid AuthorId { get; set; }

        public string DisplayName { get; set; }

        public string FullName { get; set; }

        public string Orcid { get; set; }

        public int? WorksCount { get; set; }

        public int? CitedByCount { get; set; }

        public int? HIndex { get; set; }

        public int? I10Index { get; set; }

        public decimal? TwoYearMeanCitedness { get; set; }

        public string Affiliations { get; set; }

        public string LastKnownInstitutions { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<AuthorSourceMappingResponseModel> AuthorSourceMappings { get; set; } = new List<AuthorSourceMappingResponseModel>();

        public virtual ICollection<PaperAuthorResponseModel> PaperAuthors { get; set; } = new List<PaperAuthorResponseModel>();

    }
}
