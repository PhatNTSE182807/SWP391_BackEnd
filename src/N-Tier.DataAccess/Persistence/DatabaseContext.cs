using System.Reflection;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Common;
using N_Tier.Shared.Services;

using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Persistence;

public class DatabaseContext(DbContextOptions options, IClaimService claimService) : DbContext(options)
{
    public virtual DbSet<ApiSource> ApiSources { get; set; }
    public virtual DbSet<Author> Authors { get; set; }
    public virtual DbSet<AuthorSourceMapping> AuthorSourceMappings { get; set; }
    public virtual DbSet<Journal> Journals { get; set; }
    public virtual DbSet<JournalSourceMapping> JournalSourceMappings { get; set; }
    public virtual DbSet<Keyword> Keywords { get; set; }
    public virtual DbSet<KeywordSourceMapping> KeywordSourceMappings { get; set; }
    public virtual DbSet<Paper> Papers { get; set; }
    public virtual DbSet<PaperAuthor> PaperAuthors { get; set; }
    public virtual DbSet<PaperKeyword> PaperKeywords { get; set; }
    public virtual DbSet<PaperSourceMapping> PaperSourceMappings { get; set; }
    public virtual DbSet<PipelineRun> PipelineRuns { get; set; }
    public virtual DbSet<Work> Works { get; set; }
    public virtual DbSet<Role> CoreRoles { get; set; }
    public virtual DbSet<User> CoreUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);

        builder.Entity<ApiSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK_raw_api_sources");

            entity.ToTable("api_sources", "raw");

            entity.HasIndex(e => e.SourceName, "UQ_raw_api_sources_source_name").IsUnique();

            entity.Property(e => e.SourceId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("source_id");
            entity.Property(e => e.BaseUrl)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("base_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.SourceName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("source_name");
        });

        builder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK_core_authors");

            entity.ToTable("authors", "core");

            entity.Property(e => e.AuthorId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("author_id");
            entity.Property(e => e.Affiliations).HasColumnName("affiliations");
            entity.Property(e => e.CitedByCount).HasColumnName("cited_by_count");
            entity.Property(e => e.CountsByYear).HasColumnName("counts_by_year");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("display_name");
            entity.Property(e => e.DisplayNameAlternatives).HasColumnName("display_name_alternatives");
            entity.Property(e => e.FullName)
                .HasMaxLength(500)
                .HasColumnName("full_name");
            entity.Property(e => e.HIndex).HasColumnName("h_index");
            entity.Property(e => e.I10Index).HasColumnName("i10_index");
            entity.Property(e => e.LastKnownInstitutions).HasColumnName("last_known_institutions");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(500)
                .HasComputedColumnSql("(lower(ltrim(rtrim([display_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.Orcid)
                .HasMaxLength(100)
                .HasColumnName("orcid");
            entity.Property(e => e.RawAuthorNames).HasColumnName("raw_author_names");
            entity.Property(e => e.SourceCreatedDate).HasColumnName("source_created_date");
            entity.Property(e => e.SourceUpdatedDate).HasColumnName("source_updated_date");
            entity.Property(e => e.TopicShare).HasColumnName("topic_share");
            entity.Property(e => e.Topics).HasColumnName("topics");
            entity.Property(e => e.TwoYearMeanCitedness)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("two_year_mean_citedness");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorksApiUrl)
                .HasMaxLength(1000)
                .HasColumnName("works_api_url");
            entity.Property(e => e.WorksCount).HasColumnName("works_count");
            entity.Property(e => e.XConcepts).HasColumnName("x_concepts");
        });

        builder.Entity<AuthorSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_author_source_mappings");

            entity.ToTable("author_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_author_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");
            entity.Property(e => e.SourceRecordUrl)
                .HasMaxLength(1000)
                .HasColumnName("source_record_url");
            entity.Property(e => e.SourceSpecificData).HasColumnName("source_specific_data");

            entity.HasOne(d => d.Author).WithMany(p => p.AuthorSourceMappings)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_author_source_mappings_authors");

            entity.HasOne(d => d.Source).WithMany(p => p.AuthorSourceMappings)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_author_source_mappings_api_sources");
        });

        builder.Entity<Journal>(entity =>
        {
            entity.HasKey(e => e.JournalId).HasName("PK_core_journals");

            entity.ToTable("journals", "core");

            entity.Property(e => e.JournalId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("journal_id");
            entity.Property(e => e.CitedByCount).HasColumnName("cited_by_count");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(20)
                .HasColumnName("country_code");
            entity.Property(e => e.CountsByYear).HasColumnName("counts_by_year");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FirstPublicationYear).HasColumnName("first_publication_year");
            entity.Property(e => e.HIndex).HasColumnName("h_index");
            entity.Property(e => e.HomepageUrl)
                .HasMaxLength(1000)
                .HasColumnName("homepage_url");
            entity.Property(e => e.HostOrganizationName)
                .HasMaxLength(500)
                .HasColumnName("host_organization_name");
            entity.Property(e => e.I10Index).HasColumnName("i10_index");
            entity.Property(e => e.IsCore).HasColumnName("is_core");
            entity.Property(e => e.IsInDoaj).HasColumnName("is_in_doaj");
            entity.Property(e => e.IsOpenAccess).HasColumnName("is_open_access");
            entity.Property(e => e.IssnElectronic)
                .HasMaxLength(50)
                .HasColumnName("issn_electronic");
            entity.Property(e => e.IssnL)
                .HasMaxLength(50)
                .HasColumnName("issn_l");
            entity.Property(e => e.IssnPrint)
                .HasMaxLength(50)
                .HasColumnName("issn_print");
            entity.Property(e => e.JournalName)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("journal_name");
            entity.Property(e => e.JournalType)
                .HasMaxLength(100)
                .HasColumnName("journal_type");
            entity.Property(e => e.LastPublicationYear).HasColumnName("last_publication_year");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(500)
                .HasComputedColumnSql("(lower(ltrim(rtrim([journal_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.OaWorksCount).HasColumnName("oa_works_count");
            entity.Property(e => e.Publisher)
                .HasMaxLength(500)
                .HasColumnName("publisher");
            entity.Property(e => e.SourceCreatedDate).HasColumnName("source_created_date");
            entity.Property(e => e.SourceUpdatedDate).HasColumnName("source_updated_date");
            entity.Property(e => e.Subjects).HasColumnName("subjects");
            entity.Property(e => e.Topics).HasColumnName("topics");
            entity.Property(e => e.TwoYearMeanCitedness)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("two_year_mean_citedness");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorksCount).HasColumnName("works_count");
        });

        builder.Entity<JournalSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_journal_source_mappings");

            entity.ToTable("journal_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_journal_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");
            entity.Property(e => e.SourceRecordUrl)
                .HasMaxLength(1000)
                .HasColumnName("source_record_url");
            entity.Property(e => e.SourceSpecificData).HasColumnName("source_specific_data");

            entity.HasOne(d => d.Journal).WithMany(p => p.JournalSourceMappings)
                .HasForeignKey(d => d.JournalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_journal_source_mappings_journals");

            entity.HasOne(d => d.Source).WithMany(p => p.JournalSourceMappings)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_journal_source_mappings_api_sources");
        });

        builder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.KeywordId).HasName("PK_core_keywords");

            entity.ToTable("keywords", "core");

            entity.HasIndex(e => e.NormalizedName, "UQ_core_keywords_normalized_name").IsUnique();

            entity.Property(e => e.KeywordId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("keyword_id");
            entity.Property(e => e.CitedByCount).HasColumnName("cited_by_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.KeywordName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("keyword_name");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(255)
                .HasComputedColumnSql("(lower(ltrim(rtrim([keyword_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.SourceCreatedDate).HasColumnName("source_created_date");
            entity.Property(e => e.SourceUpdatedDate).HasColumnName("source_updated_date");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorksApiUrl)
                .HasMaxLength(1000)
                .HasColumnName("works_api_url");
            entity.Property(e => e.WorksCount).HasColumnName("works_count");
        });

        builder.Entity<KeywordSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_keyword_source_mappings");

            entity.ToTable("keyword_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_keyword_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.KeywordId).HasColumnName("keyword_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");
            entity.Property(e => e.SourceRecordUrl)
                .HasMaxLength(1000)
                .HasColumnName("source_record_url");
            entity.Property(e => e.SourceSpecificData).HasColumnName("source_specific_data");

            entity.HasOne(d => d.Keyword).WithMany(p => p.KeywordSourceMappings)
                .HasForeignKey(d => d.KeywordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_keyword_source_mappings_keywords");

            entity.HasOne(d => d.Source).WithMany(p => p.KeywordSourceMappings)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_keyword_source_mappings_api_sources");
        });

        builder.Entity<Paper>(entity =>
        {
            entity.HasKey(e => e.PaperId).HasName("PK_core_papers");

            entity.ToTable("papers", "core");

            entity.Property(e => e.PaperId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("paper_id");
            entity.Property(e => e.Abstract).HasColumnName("abstract");
            entity.Property(e => e.AbstractInvertedIndex).HasColumnName("abstract_inverted_index");
            entity.Property(e => e.CitedByCount).HasColumnName("cited_by_count");
            entity.Property(e => e.CountsByYear).HasColumnName("counts_by_year");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Doi)
                .HasMaxLength(500)
                .HasColumnName("doi");
            entity.Property(e => e.IsOpenAccess).HasColumnName("is_open_access");
            entity.Property(e => e.IsRetracted).HasColumnName("is_retracted");
            entity.Property(e => e.Issue)
                .HasMaxLength(50)
                .HasColumnName("issue");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.Language)
                .HasMaxLength(20)
                .HasColumnName("language");
            entity.Property(e => e.Page)
                .HasMaxLength(100)
                .HasColumnName("page");
            entity.Property(e => e.PaperType)
                .HasMaxLength(100)
                .HasColumnName("paper_type");
            entity.Property(e => e.PublicationDate).HasColumnName("publication_date");
            entity.Property(e => e.PublicationYear).HasColumnName("publication_year");
            entity.Property(e => e.ReferenceCount).HasColumnName("reference_count");
            entity.Property(e => e.ReferencedWorks).HasColumnName("referenced_works");
            entity.Property(e => e.RelatedWorks).HasColumnName("related_works");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Volume)
                .HasMaxLength(50)
                .HasColumnName("volume");

            entity.HasOne(d => d.Journal).WithMany(p => p.Papers)
                .HasForeignKey(d => d.JournalId)
                .HasConstraintName("FK_core_papers_journals");
        });

        builder.Entity<PaperAuthor>(entity =>
        {
            entity.HasKey(e => e.PaperAuthorId).HasName("PK_core_paper_authors");

            entity.ToTable("paper_authors", "core");

            entity.HasIndex(e => new { e.PaperId, e.AuthorId }, "UQ_core_paper_authors_paper_author").IsUnique();

            entity.Property(e => e.PaperAuthorId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("paper_author_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.AuthorOrder).HasColumnName("author_order");
            entity.Property(e => e.AuthorPosition)
                .HasMaxLength(50)
                .HasColumnName("author_position");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCorresponding).HasColumnName("is_corresponding");
            entity.Property(e => e.PaperId).HasColumnName("paper_id");
            entity.Property(e => e.RawAuthorName)
                .HasMaxLength(500)
                .HasColumnName("raw_author_name");

            entity.HasOne(d => d.Author).WithMany(p => p.PaperAuthors)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_authors_authors");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperAuthors)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_authors_papers");
        });

        builder.Entity<PaperKeyword>(entity =>
        {
            entity.HasKey(e => e.PaperKeywordId).HasName("PK_core_paper_keywords");

            entity.ToTable("paper_keywords", "core");

            entity.HasIndex(e => new { e.PaperId, e.KeywordId, e.SourceId }, "UQ_core_paper_keywords_paper_keyword_source").IsUnique();

            entity.Property(e => e.PaperKeywordId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("paper_keyword_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.KeywordId).HasColumnName("keyword_id");
            entity.Property(e => e.PaperId).HasColumnName("paper_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(10, 6)")
                .HasColumnName("score");
            entity.Property(e => e.SourceId).HasColumnName("source_id");

            entity.HasOne(d => d.Keyword).WithMany(p => p.PaperKeywords)
                .HasForeignKey(d => d.KeywordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_keywords_keywords");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperKeywords)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_keywords_papers");

            entity.HasOne(d => d.Source).WithMany(p => p.PaperKeywords)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_keywords_api_sources");
        });

        builder.Entity<PaperSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_paper_source_mappings");

            entity.ToTable("paper_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_paper_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.PaperId).HasColumnName("paper_id");
            entity.Property(e => e.RawWorkId).HasColumnName("raw_work_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");
            entity.Property(e => e.SourceRecordUrl)
                .HasMaxLength(1000)
                .HasColumnName("source_record_url");
            entity.Property(e => e.SourceSpecificData).HasColumnName("source_specific_data");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperSourceMappings)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_source_mappings_papers");

            entity.HasOne(d => d.RawWork).WithMany(p => p.PaperSourceMappings)
                .HasForeignKey(d => d.RawWorkId)
                .HasConstraintName("FK_core_paper_source_mappings_raw_works");

            entity.HasOne(d => d.Source).WithMany(p => p.PaperSourceMappings)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_source_mappings_api_sources");
        });

        builder.Entity<PipelineRun>(entity =>
        {
            entity.HasKey(e => e.RunId).HasName("PK_raw_pipeline_runs");

            entity.ToTable("pipeline_runs", "raw");

            entity.Property(e => e.RunId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("run_id");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.FinishedAt).HasColumnName("finished_at");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(255)
                .HasColumnName("query_keyword");
            entity.Property(e => e.RecordsFailed).HasColumnName("records_failed");
            entity.Property(e => e.RecordsFetched).HasColumnName("records_fetched");
            entity.Property(e => e.RecordsInserted).HasColumnName("records_inserted");
            entity.Property(e => e.SourceEntity)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("works")
                .HasColumnName("source_entity");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("running")
                .HasColumnName("status");

            entity.HasOne(d => d.Source).WithMany(p => p.PipelineRuns)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_raw_pipeline_runs_api_sources");
        });

        builder.Entity<Work>(entity =>
        {
            entity.HasKey(e => e.RawWorkId).HasName("PK_raw_works");

            entity.ToTable("works", "raw");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_raw_works_source_record").IsUnique();

            entity.Property(e => e.RawWorkId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("raw_work_id");
            entity.Property(e => e.FetchedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fetched_at");
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(e => e.PipelineRunId).HasColumnName("pipeline_run_id");
            entity.Property(e => e.ProcessError).HasColumnName("process_error");
            entity.Property(e => e.ProcessedStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("pending")
                .HasColumnName("processed_status");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(255)
                .HasColumnName("query_keyword");
            entity.Property(e => e.RawData)
                .IsRequired()
                .HasColumnName("raw_data");
            entity.Property(e => e.SourceEntity)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("works")
                .HasColumnName("source_entity");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");

            entity.HasOne(d => d.PipelineRun).WithMany(p => p.Works)
                .HasForeignKey(d => d.PipelineRunId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_raw_works_pipeline_runs");

            entity.HasOne(d => d.Source).WithMany(p => p.Works)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_raw_works_api_sources");
        });

        builder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_core_roles");

            entity.ToTable("roles", "core");

            entity.HasIndex(e => e.RoleName, "UQ_core_roles_role_name").IsUnique();

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_core_users");

            entity.ToTable("users", "core");

            entity.HasIndex(e => e.Email, "UQ_core_users_email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_core_users_username").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("user_id");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("password");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(50)
                .HasColumnName("phonenumber");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_users_roles");
        });
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = claimService.GetUserId();
                    entry.Entity.CreatedOn = DateTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedBy = claimService.GetUserId();
                    entry.Entity.UpdatedOn = DateTime.Now;
                    break;
            }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
