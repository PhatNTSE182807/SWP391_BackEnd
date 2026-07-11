using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models.Author;
using N_Tier.Application.Models.Journal;
using N_Tier.Application.Models.JournalTopic;
using N_Tier.Application.Models.JournalType;
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
                .Map(dest => dest.Journal, src => src.Journal != null ? new PaperJournalSummaryResponseModel
                {
                    JournalId = src.Journal.JournalId,
                    JournalName = src.Journal.JournalName
                } : null)
                .Map(dest => dest.PaperAuthors, src => src.PaperAuthors.Select(pa => new PaperAuthorSummaryResponseModel
                {
                    AuthorId = pa.AuthorId,
                    AuthorName = pa.Author.DisplayName ?? pa.Author.FullName ?? pa.RawAuthorName
                }))
                .Map(dest => dest.PaperTopics, src => src.PaperTopics.Select(pt => new PaperTopicSummaryResponseModel
                {
                    TopicId = pt.TopicId,
                    TopicName = pt.Topic.TopicName
                }))
                .Map(dest => dest.PaperKeywords, src => src.PaperKeywords.Select(pk => new PaperKeywordSummaryResponseModel
                {
                    KeywordId = pk.KeywordId,
                    KeywordName = pk.Keyword.KeywordName
                }))
                .Map(dest => dest.UserBookmarks, src => src.UserBookmarks);

            config.NewConfig<Journal, JournalResponseModel>()
                .Map(dest => dest.JournalId, src => src.JournalId)
                .Map(dest => dest.Topics, src => src.JournalTopics.Select(jt => new JournalTopicSimpleModel
                {
                    JournalTopicId = jt.JournalTopicId,
                    TopicId = jt.TopicId,
                    TopicName = jt.Topic != null ? jt.Topic.TopicName : null,
                    WorksCount = jt.WorksCount,
                    TopicShare = jt.TopicShare
                }))
                .Map(dest => dest.JournalTypeNavigation, src => src.JournalTypeNavigation)
                .Map(dest => dest.Papers, src => src.Papers.Select(p => new PaperResponseModel
                {
                    PaperId = p.PaperId,
                    Doi = p.Doi,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    PublicationYear = p.PublicationYear,
                    PublicationDate = p.PublicationDate,
                    PaperType = p.PaperType,
                    Language = p.Language,
                    CitedByCount = p.CitedByCount,
                    ReferenceCount = p.ReferenceCount,
                    Volume = p.Volume,
                    Issue = p.Issue,
                    Page = p.Page,
                    IsOpenAccess = p.IsOpenAccess,
                    IsRetracted = p.IsRetracted,
                    JournalId = p.JournalId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    PaperAuthors = p.PaperAuthors.Select(pa => new PaperAuthorSummaryResponseModel
                    {
                        AuthorId = pa.AuthorId,
                        AuthorName = pa.Author.DisplayName ?? pa.Author.FullName ?? pa.RawAuthorName
                    }).ToList()
                }));

            config.NewConfig<PaperAuthor, PaperAuthorResponseModel>()
                .Map(dest => dest.PaperAuthorId, src => src.PaperAuthorId)
                .Map(dest => dest.Author, src => src.Author)
                .Map(dest => dest.Paper, src => src.Paper);

            config.NewConfig<Author, AuthorResponseModel>()
                .Map(dest => dest.AuthorId, src => src.AuthorId)
                .Map(dest => dest.PaperAuthors, src => src.PaperAuthors.Select(pa => new PaperAuthorResponseModel
                {
                    PaperAuthorId = pa.PaperAuthorId,
                    AuthorId = pa.AuthorId,
                    AuthorOrder = pa.AuthorOrder,
                    AuthorPosition = pa.AuthorPosition,
                    RawAuthorName = pa.RawAuthorName,
                    IsCorresponding = pa.IsCorresponding,
                    CreatedAt = pa.CreatedAt,
                    Paper = new PaperResponseModel
                    {
                        PaperId = pa.Paper.PaperId,
                        Doi = pa.Paper.Doi,
                        Title = pa.Paper.Title,
                        Abstract = pa.Paper.Abstract,
                        PublicationYear = pa.Paper.PublicationYear,
                        PublicationDate = pa.Paper.PublicationDate,
                        PaperType = pa.Paper.PaperType,
                        Language = pa.Paper.Language,
                        CitedByCount = pa.Paper.CitedByCount,
                        ReferenceCount = pa.Paper.ReferenceCount,
                        Volume = pa.Paper.Volume,
                        Issue = pa.Paper.Issue,
                        Page = pa.Paper.Page,
                        IsOpenAccess = pa.Paper.IsOpenAccess,
                        IsRetracted = pa.Paper.IsRetracted,
                        JournalId = pa.Paper.JournalId,
                        CreatedAt = pa.Paper.CreatedAt,
                        UpdatedAt = pa.Paper.UpdatedAt,
                        Journal = pa.Paper.Journal != null ? new PaperJournalSummaryResponseModel
                        {
                            JournalId = pa.Paper.Journal.JournalId,
                            JournalName = pa.Paper.Journal.JournalName
                        } : null
                    }
                }));

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


            config.NewConfig<JournalTopic, JournalTopicResponseModel>()
                .Map(dest => dest.JournalTopicId, src => src.JournalTopicId)
                .Map(dest => dest.Topic, src => src.Topic);

            config.NewConfig<JournalType, JournalTypeResponseModel>()
                .Map(dest => dest.JournalTypeId, src => src.JournalTypeId);
        }

    }
}
