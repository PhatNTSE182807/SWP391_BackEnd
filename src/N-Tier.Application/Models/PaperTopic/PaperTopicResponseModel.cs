using System;
using N_Tier.Application.Models.Topic;

namespace N_Tier.Application.Models.PaperTopic
{
    public class PaperTopicResponseModel
    {
        public Guid PaperTopicId { get; set; }

        public Guid TopicId { get; set; }

        public decimal? Score { get; set; }

        public Guid SourceId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual TopicResponseModel Topic { get; set; }
    }
}
