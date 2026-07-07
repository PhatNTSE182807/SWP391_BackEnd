using System;

namespace N_Tier.Application.Models.JournalSourceMapping
{
    public class JournalSourceMappingResponseModel
    {
        public Guid JournalSourceMappingId { get; set; }

        public Guid SourceId { get; set; }

        public Guid? RawSourceId { get; set; }

        public string SourceRecordId { get; set; }

        public string SourceRecordUrl { get; set; }

        public string SourceSpecificData { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
