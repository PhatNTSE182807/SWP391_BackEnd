using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N_Tier.Application.Models.Journal;
using N_Tier.Application.Models.PaperAuthor;
using N_Tier.Core.Entities;

namespace N_Tier.Application.Models.Paper
{
    public class PaperResponseModel : BaseResponseModel
    {
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

        public virtual JournalResponseModel Journal { get; set; }

        public virtual ICollection<PaperAuthorResponseModel> PaperAuthorResponseModels { get; set; } = new List<PaperAuthorResponseModel>();

    }
}
