using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N_Tier.Application.Models.Author;
using N_Tier.Application.Models.Paper;
using N_Tier.Core.Entities;

namespace N_Tier.Application.Models.PaperAuthor
{
    public class PaperAuthorResponseModel : BaseResponseModel
    {
        public Guid AuthorId { get; set; }

        public int? AuthorOrder { get; set; }

        public string AuthorPosition { get; set; }

        public string RawAuthorName { get; set; }

        public bool? IsCorresponding { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual AuthorResponseModel Author { get; set; }

    }
}
