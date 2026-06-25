using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Topic;

namespace N_Tier.Application.Services
{
    public interface ITopicService
    {
        Task<List<TopicResponseModel>> GetAllTopicsAsync();
        Task<TopicResponseModel> GetTopicByIdAsync(Guid id);
    }
}
