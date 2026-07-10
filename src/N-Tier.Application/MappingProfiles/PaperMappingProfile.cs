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
                .Map(dest => dest.Journal, src => src.Journal != null ? new JournalResponseModel
                {
                    JournalId = src.Journal.JournalId,
                    JournalName = src.Journal.JournalName,
                    NormalizedName = src.Journal.NormalizedName,
                    IssnL = src.Journal.IssnL,
                    IssnPrint = src.Journal.IssnPrint,
                    IssnElectronic = src.Journal.IssnElectronic,
                    Publisher = src.Journal.Publisher,
                    HostOrganizationName = src.Journal.HostOrganizationName,
                    JournalType = src.Journal.JournalType,
                    JournalTypeId = src.Journal.JournalTypeId,
                    HomepageUrl = src.Journal.HomepageUrl,
                    CountryCode = src.Journal.CountryCode,
                    WorksCount = src.Journal.WorksCount,
                    CitedByCount = src.Journal.CitedByCount,
                    OaWorksCount = src.Journal.OaWorksCount,
                    HIndex = src.Journal.HIndex,
                    I10Index = src.Journal.I10Index,
                    TwoYearMeanCitedness = src.Journal.TwoYearMeanCitedness,
                    IsOpenAccess = src.Journal.IsOpenAccess,
                    IsInDoaj = src.Journal.IsInDoaj,
                    IsCore = src.Journal.IsCore,
                    FirstPublicationYear = src.Journal.FirstPublicationYear,
                    LastPublicationYear = src.Journal.LastPublicationYear,
                    CountsByYear = src.Journal.CountsByYear,
                    SourceCreatedDate = src.Journal.SourceCreatedDate,
                    SourceUpdatedDate = src.Journal.SourceUpdatedDate,
                    CreatedAt = src.Journal.CreatedAt,
                    UpdatedAt = src.Journal.UpdatedAt
                } : null)
                .Map(dest => dest.PaperAuthors, src => src.PaperAuthors.Select(pa => new PaperAuthorResponseModel
                {
                    PaperAuthorId = pa.PaperAuthorId,
                    AuthorId = pa.AuthorId,
                    AuthorOrder = pa.AuthorOrder,
                    AuthorPosition = pa.AuthorPosition,
                    RawAuthorName = pa.RawAuthorName,
                    IsCorresponding = pa.IsCorresponding,
                    CreatedAt = pa.CreatedAt,
                    Author = new AuthorResponseModel
                    {
                        AuthorId = pa.Author.AuthorId,
                        DisplayName = pa.Author.DisplayName,
                        NormalizedName = pa.Author.NormalizedName,
                        FullName = pa.Author.FullName,
                        Orcid = pa.Author.Orcid,
                        WorksCount = pa.Author.WorksCount,
                        CitedByCount = pa.Author.CitedByCount,
                        HIndex = pa.Author.HIndex,
                        I10Index = pa.Author.I10Index,
                        TwoYearMeanCitedness = pa.Author.TwoYearMeanCitedness,
                        RawAuthorNames = pa.Author.RawAuthorNames,
                        DisplayNameAlternatives = pa.Author.DisplayNameAlternatives,
                        Affiliations = pa.Author.Affiliations,
                        LastKnownInstitutions = pa.Author.LastKnownInstitutions,
                        Topics = pa.Author.Topics,
                        TopicShare = pa.Author.TopicShare,
                        XConcepts = pa.Author.XConcepts,
                        CountsByYear = pa.Author.CountsByYear,
                        WorksApiUrl = pa.Author.WorksApiUrl,
                        SourceCreatedDate = pa.Author.SourceCreatedDate,
                        SourceUpdatedDate = pa.Author.SourceUpdatedDate,
                        CreatedAt = pa.Author.CreatedAt,
                        UpdatedAt = pa.Author.UpdatedAt
                    }
                }))
                .Map(dest => dest.PaperTopics, src => src.PaperTopics)
                .Map(dest => dest.PaperKeywords, src => src.PaperKeywords)
                .Map(dest => dest.UserBookmarks, src => src.UserBookmarks);

            config.NewConfig<Journal, JournalResponseModel>()
                .Map(dest => dest.JournalId, src => src.JournalId)
                .Map(dest => dest.JournalTopics, src => src.JournalTopics)
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
                    PaperAuthors = p.PaperAuthors.Select(pa => new PaperAuthorResponseModel
                    {
                        PaperAuthorId = pa.PaperAuthorId,
                        AuthorId = pa.AuthorId,
                        AuthorOrder = pa.AuthorOrder,
                        AuthorPosition = pa.AuthorPosition,
                        RawAuthorName = pa.RawAuthorName,
                        IsCorresponding = pa.IsCorresponding,
                        CreatedAt = pa.CreatedAt,
                        Author = new AuthorResponseModel
                        {
                            AuthorId = pa.Author.AuthorId,
                            DisplayName = pa.Author.DisplayName,
                            NormalizedName = pa.Author.NormalizedName,
                            FullName = pa.Author.FullName,
                            Orcid = pa.Author.Orcid,
                            WorksCount = pa.Author.WorksCount,
                            CitedByCount = pa.Author.CitedByCount,
                            HIndex = pa.Author.HIndex,
                            I10Index = pa.Author.I10Index,
                            TwoYearMeanCitedness = pa.Author.TwoYearMeanCitedness,
                            RawAuthorNames = pa.Author.RawAuthorNames,
                            DisplayNameAlternatives = pa.Author.DisplayNameAlternatives,
                            Affiliations = pa.Author.Affiliations,
                            LastKnownInstitutions = pa.Author.LastKnownInstitutions,
                            Topics = pa.Author.Topics,
                            TopicShare = pa.Author.TopicShare,
                            XConcepts = pa.Author.XConcepts,
                            CountsByYear = pa.Author.CountsByYear,
                            WorksApiUrl = pa.Author.WorksApiUrl,
                            SourceCreatedDate = pa.Author.SourceCreatedDate,
                            SourceUpdatedDate = pa.Author.SourceUpdatedDate,
                            CreatedAt = pa.Author.CreatedAt,
                            UpdatedAt = pa.Author.UpdatedAt
                        }
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
                        Journal = pa.Paper.Journal != null ? new JournalResponseModel
                        {
                            JournalId = pa.Paper.Journal.JournalId,
                            JournalName = pa.Paper.Journal.JournalName,
                            NormalizedName = pa.Paper.Journal.NormalizedName,
                            IssnL = pa.Paper.Journal.IssnL,
                            IssnPrint = pa.Paper.Journal.IssnPrint,
                            IssnElectronic = pa.Paper.Journal.IssnElectronic,
                            Publisher = pa.Paper.Journal.Publisher,
                            HostOrganizationName = pa.Paper.Journal.HostOrganizationName,
                            JournalType = pa.Paper.Journal.JournalType,
                            JournalTypeId = pa.Paper.Journal.JournalTypeId,
                            HomepageUrl = pa.Paper.Journal.HomepageUrl,
                            CountryCode = pa.Paper.Journal.CountryCode,
                            WorksCount = pa.Paper.Journal.WorksCount,
                            CitedByCount = pa.Paper.Journal.CitedByCount,
                            OaWorksCount = pa.Paper.Journal.OaWorksCount,
                            HIndex = pa.Paper.Journal.HIndex,
                            I10Index = pa.Paper.Journal.I10Index,
                            TwoYearMeanCitedness = pa.Paper.Journal.TwoYearMeanCitedness,
                            IsOpenAccess = pa.Paper.Journal.IsOpenAccess,
                            IsInDoaj = pa.Paper.Journal.IsInDoaj,
                            IsCore = pa.Paper.Journal.IsCore,
                            FirstPublicationYear = pa.Paper.Journal.FirstPublicationYear,
                            LastPublicationYear = pa.Paper.Journal.LastPublicationYear,
                            CountsByYear = pa.Paper.Journal.CountsByYear,
                            SourceCreatedDate = pa.Paper.Journal.SourceCreatedDate,
                            SourceUpdatedDate = pa.Paper.Journal.SourceUpdatedDate,
                            CreatedAt = pa.Paper.Journal.CreatedAt,
                            UpdatedAt = pa.Paper.Journal.UpdatedAt
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
