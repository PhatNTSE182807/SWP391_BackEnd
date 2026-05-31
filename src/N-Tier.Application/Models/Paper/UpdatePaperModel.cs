using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N_Tier.Application.Models.Paper
{
    public class UpdatePaperModel
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string PaperType { get; set; }
        public string Language { get; set; }
        public bool? IsOpenAccess { get; set; }
        public bool? IsRetracted { get; set; }
        public Guid? JournalId { get; set; }
    }
}
