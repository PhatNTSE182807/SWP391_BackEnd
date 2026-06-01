using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.Author;
using N_Tier.Application.Models.Journal;
using N_Tier.Application.Models.Paper;
using N_Tier.Application.Models.PaperAuthor;
using N_Tier.Core.Entities;

namespace N_Tier.Application.MappingProfiles
{
    public class PaperMappingProfile : IRegister, IMappingProfilesMarker
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Paper, PaperResponseModel>()
                .Map(dest => dest.Id, src => src.PaperId)
                .Map(dest => dest.Journal, src => src.Journal)
                .Map(dest => dest.PaperAuthorResponseModels, src => src.PaperAuthors);

            config.NewConfig<Journal, JournalResponseModel>()
                .Map(dest => dest.Id, src => src.JournalId);

            config.NewConfig<PaperAuthor, PaperAuthorResponseModel>()
                .Map(dest => dest.Id, src => src.PaperAuthorId)
                .Map(dest => dest.Author, src => src.Author);

            config.NewConfig<Author, AuthorResponseModel>()
                .Map(dest => dest.Id, src => src.AuthorId);
        }

    }
}
