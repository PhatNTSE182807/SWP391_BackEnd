using System;

namespace N_Tier.Application.Models.JournalType
{
    public class JournalTypeResponseModel
    {
        public Guid JournalTypeId { get; set; }

        public string TypeCode { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
