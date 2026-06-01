

namespace N_Tier.Application.Models.Author
{
    public class AuthorResponseModel : BaseResponseModel
    {

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
    }
}
