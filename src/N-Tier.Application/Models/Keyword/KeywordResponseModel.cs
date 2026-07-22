using System;

namespace N_Tier.Application.Models.Keyword
{
    public class KeywordResponseModel
    {
        public Guid KeywordId { get; set; }

        public string KeywordName { get; set; }

        public string NormalizedName { get; set; }

        public int? WorksCount { get; set; }

        public int? CitedByCount { get; set; }

        public string WorksApiUrl { get; set; }

        public DateOnly? SourceCreatedDate { get; set; }

        public DateTime? SourceUpdatedDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
