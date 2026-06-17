using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.Topic;
using N_Tier.DataAccess.Repositories;

namespace N_Tier.Application.Services.Impl
{
    public class TopicService : ITopicService
    {
        private readonly IResearchTopicRepository _researchTopicRepository;

        public TopicService(IResearchTopicRepository researchTopicRepository)
        {
            _researchTopicRepository = researchTopicRepository;
        }

        public async Task<List<TopicResponseModel>> GetAllTopicsAsync()
        {
            var topics = await _researchTopicRepository.GetAllAsync(t => true);
            return topics.Adapt<List<TopicResponseModel>>();
        }

        public async Task<TopicResponseModel> GetTopicByIdAsync(Guid id)
        {
            var topic = await _researchTopicRepository.GetFirstAsync(t => t.TopicId == id);
            return topic.Adapt<TopicResponseModel>();
        }
    }
}
