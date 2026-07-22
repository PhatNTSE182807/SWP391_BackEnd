using System;
using N_Tier.Application.Models.Keyword;

namespace N_Tier.Application.Models.PaperKeyword
{
    public class PaperKeywordResponseModel
    {
        public Guid PaperKeywordId { get; set; }

        public Guid KeywordId { get; set; }

        public decimal? Score { get; set; }

        public Guid SourceId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual KeywordResponseModel Keyword { get; set; }
    }
}
