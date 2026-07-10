using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N_Tier.Application.Models.PaperKeyword;
using N_Tier.Application.Models.PaperTopic;
using N_Tier.Application.Models.UserBookmark;

namespace N_Tier.Application.Models.Paper
{
    public class PaperResponseModel
    {
        public Guid PaperId { get; set; }

        public string Doi { get; set; }

        public string Title { get; set; }

        public string Abstract { get; set; }

        public int? PublicationYear { get; set; }

        public DateOnly? PublicationDate { get; set; }

        public string PaperType { get; set; }

        public string Language { get; set; }

        public int? CitedByCount { get; set; }

        public int? ReferenceCount { get; set; }

        public string Volume { get; set; }

        public string Issue { get; set; }

        public string Page { get; set; }

        public bool? IsOpenAccess { get; set; }

        public bool? IsRetracted { get; set; }

        public Guid? JournalId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual PaperJournalSummaryResponseModel Journal { get; set; }

        public virtual ICollection<PaperAuthorSummaryResponseModel> PaperAuthors { get; set; } = new List<PaperAuthorSummaryResponseModel>();

        public virtual ICollection<PaperTopicResponseModel> PaperTopics { get; set; } = new List<PaperTopicResponseModel>();

        public virtual ICollection<PaperKeywordResponseModel> PaperKeywords { get; set; } = new List<PaperKeywordResponseModel>();

        public virtual ICollection<UserBookmarkResponseModel> UserBookmarks { get; set; } = new List<UserBookmarkResponseModel>();

    }

    public class PaperJournalSummaryResponseModel
    {
        public Guid JournalId { get; set; }

        public string JournalName { get; set; }
    }

    public class PaperAuthorSummaryResponseModel
    {
        public Guid AuthorId { get; set; }

        public string AuthorName { get; set; }
    }
}
