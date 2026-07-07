using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.Author;
using N_Tier.Application.Models.Journal;
using N_Tier.Application.Models.JournalSourceMapping;
using N_Tier.Application.Models.JournalTopic;
using N_Tier.Application.Models.JournalType;
using N_Tier.Application.Models.AuthorSourceMapping;
using N_Tier.Application.Models.Keyword;
using N_Tier.Application.Models.Paper;
using N_Tier.Application.Models.PaperAuthor;
using N_Tier.Application.Models.PaperKeyword;
using N_Tier.Application.Models.PaperSourceMapping;
using N_Tier.Application.Models.PaperTopic;
using N_Tier.Application.Models.User;
using N_Tier.Application.Models.UserBookmark;
using N_Tier.Core.Entities;

namespace N_Tier.Application.MappingProfiles
{
    public class PaperMappingProfile : IRegister, IMappingProfilesMarker
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Paper, PaperResponseModel>()
                .Map(dest => dest.PaperId, src => src.PaperId)
                .Map(dest => dest.Journal, src => src.Journal)
                .Map(dest => dest.PaperAuthors, src => src.PaperAuthors)
                .Map(dest => dest.PaperTopics, src => src.PaperTopics)
                .Map(dest => dest.PaperKeywords, src => src.PaperKeywords)
                .Map(dest => dest.PaperSourceMappings, src => src.PaperSourceMappings)
                .Map(dest => dest.UserBookmarks, src => src.UserBookmarks);

            config.NewConfig<Journal, JournalResponseModel>()
                .Map(dest => dest.JournalId, src => src.JournalId)
                .Map(dest => dest.JournalSourceMappings, src => src.JournalSourceMappings)
                .Map(dest => dest.JournalTopics, src => src.JournalTopics)
                .Map(dest => dest.JournalTypeNavigation, src => src.JournalTypeNavigation)
                .Map(dest => dest.Papers, src => src.Papers);

            config.NewConfig<PaperAuthor, PaperAuthorResponseModel>()
                .Map(dest => dest.PaperAuthorId, src => src.PaperAuthorId)
                .Map(dest => dest.Author, src => src.Author)
                .Map(dest => dest.Paper, src => src.Paper);

            config.NewConfig<Author, AuthorResponseModel>()
                .Map(dest => dest.AuthorId, src => src.AuthorId)
                .Map(dest => dest.AuthorSourceMappings, src => src.AuthorSourceMappings)
                .Map(dest => dest.PaperAuthors, src => src.PaperAuthors);

            config.NewConfig<PaperTopic, PaperTopicResponseModel>()
                .Map(dest => dest.PaperTopicId, src => src.PaperTopicId)
                .Map(dest => dest.Topic, src => src.Topic);

            config.NewConfig<PaperKeyword, PaperKeywordResponseModel>()
                .Map(dest => dest.PaperKeywordId, src => src.PaperKeywordId)
                .Map(dest => dest.Keyword, src => src.Keyword);

            config.NewConfig<Keyword, KeywordResponseModel>()
                .Map(dest => dest.KeywordId, src => src.KeywordId);

            config.NewConfig<PaperSourceMapping, PaperSourceMappingResponseModel>()
                .Map(dest => dest.PaperSourceMappingId, src => src.MappingId);

            config.NewConfig<UserBookmark, N_Tier.Application.Models.UserBookmark.UserBookmarkResponseModel>()
                .Map(dest => dest.UserBookmarkId, src => src.BookmarkId)
                .Map(dest => dest.User, src => src.User);

            config.NewConfig<User, UserResponseModel>()
                .Map(dest => dest.UserId, src => src.UserId)
                .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.RoleName : null);

            config.NewConfig<JournalSourceMapping, JournalSourceMappingResponseModel>()
                .Map(dest => dest.JournalSourceMappingId, src => src.MappingId);

            config.NewConfig<JournalTopic, JournalTopicResponseModel>()
                .Map(dest => dest.JournalTopicId, src => src.JournalTopicId)
                .Map(dest => dest.Topic, src => src.Topic);

            config.NewConfig<JournalType, JournalTypeResponseModel>()
                .Map(dest => dest.JournalTypeId, src => src.JournalTypeId);

            config.NewConfig<AuthorSourceMapping, AuthorSourceMappingResponseModel>()
                .Map(dest => dest.AuthorSourceMappingId, src => src.MappingId);
        }

    }
}
