using Mapster;
using N_Tier.Application.Models.Topic;
using N_Tier.Core.Entities;

namespace N_Tier.Application.MappingProfiles
{
    public class TopicMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ResearchTopic, TopicResponseModel>()
                .Map(dest => dest.Id, src => src.TopicId);
        }
    }
}
