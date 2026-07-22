using System;
using N_Tier.Application.Models.Topic;

namespace N_Tier.Application.Models.JournalTopic
{
    public class JournalTopicResponseModel
    {
        public Guid JournalTopicId { get; set; }

        public Guid TopicId { get; set; }

        public int? WorksCount { get; set; }

        public decimal? TopicShare { get; set; }

        public Guid SourceId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual TopicResponseModel Topic { get; set; }
    }
}
