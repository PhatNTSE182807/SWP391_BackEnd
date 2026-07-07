using System;

namespace N_Tier.Application.Models.PaperSourceMapping
{
    public class PaperSourceMappingResponseModel
    {
        public Guid PaperSourceMappingId { get; set; }

        public Guid SourceId { get; set; }

        public Guid? RawWorkId { get; set; }

        public string SourceRecordId { get; set; }

        public string SourceRecordUrl { get; set; }

        public string SourceSpecificData { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
