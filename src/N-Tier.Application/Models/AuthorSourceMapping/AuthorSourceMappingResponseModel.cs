using System;

namespace N_Tier.Application.Models.AuthorSourceMapping
{
    public class AuthorSourceMappingResponseModel
    {
        public Guid AuthorSourceMappingId { get; set; }

        public Guid SourceId { get; set; }

        public Guid? RawAuthorId { get; set; }

        public string SourceRecordId { get; set; }

        public string SourceRecordUrl { get; set; }

        public string SourceSpecificData { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
