using System;
using N_Tier.Application.Models;

namespace N_Tier.Application.Models.Topic
{
    public class TopicResponseModel : BaseResponseModel
    {
        public Guid? SubfieldId { get; set; }

        public string TopicName { get; set; }

        public string NormalizedName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
