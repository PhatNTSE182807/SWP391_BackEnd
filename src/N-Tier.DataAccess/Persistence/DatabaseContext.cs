using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Persistence;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<AuthorSourceMapping> AuthorSourceMappings { get; set; }

    public virtual DbSet<DomainSourceMapping> DomainSourceMappings { get; set; }

    public virtual DbSet<FieldSourceMapping> FieldSourceMappings { get; set; }

    public virtual DbSet<Journal> Journals { get; set; }

    public virtual DbSet<JournalSourceMapping> JournalSourceMappings { get; set; }

    public virtual DbSet<JournalTopic> JournalTopics { get; set; }

    public virtual DbSet<JournalType> JournalTypes { get; set; }

    public virtual DbSet<Keyword> Keywords { get; set; }

    public virtual DbSet<KeywordSourceMapping> KeywordSourceMappings { get; set; }

    public virtual DbSet<Paper> Papers { get; set; }

    public virtual DbSet<PaperAuthor> PaperAuthors { get; set; }

    public virtual DbSet<PaperKeyword> PaperKeywords { get; set; }

    public virtual DbSet<PaperSourceMapping> PaperSourceMappings { get; set; }

    public virtual DbSet<PaperTopic> PaperTopics { get; set; }

    public virtual DbSet<ResearchDomain> ResearchDomains { get; set; }

    public virtual DbSet<ResearchField> ResearchFields { get; set; }

    public virtual DbSet<ResearchSubfield> ResearchSubfields { get; set; }

    public virtual DbSet<ResearchTopic> ResearchTopics { get; set; }

    public virtual DbSet<Role> CoreRoles { get; set; }

    public virtual DbSet<User> CoreUsers { get; set; }

    public virtual DbSet<SubfieldSourceMapping> SubfieldSourceMappings { get; set; }

    public virtual DbSet<TopicSourceMapping> TopicSourceMappings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=13.213.7.89,1433;Database=scientific_journal_tracking_db;User ID=backend_user;Password=NguyeN2004@;TrustServerCertificate=True;",
                    opt => opt.MigrationsHistoryTable("__EFMigrationsHistory", "core"));
            }
        }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
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
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
            entity.Property(e => e.TwoYearMeanCitedness).HasColumnName("two_year_mean_citedness");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorksApiUrl)
                .HasMaxLength(1000)
                .HasColumnName("works_api_url");
            entity.Property(e => e.WorksCount).HasColumnName("works_count");
            entity.Property(e => e.XConcepts).HasColumnName("x_concepts");
        });

        modelBuilder.Entity<AuthorSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_author_source_mappings");

            entity.ToTable("author_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_author_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.RawAuthorId).HasColumnName("raw_author_id");
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
        });

        modelBuilder.Entity<DomainSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_domain_source_mappings");

            entity.ToTable("domain_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_domain_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.DomainId).HasColumnName("domain_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");
            entity.Property(e => e.SourceRecordUrl)
                .HasMaxLength(1000)
                .HasColumnName("source_record_url");
            entity.Property(e => e.SourceSpecificData).HasColumnName("source_specific_data");

            entity.HasOne(d => d.Domain).WithMany(p => p.DomainSourceMappings)
                .HasForeignKey(d => d.DomainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_domain_source_mappings_domains");
        });

        modelBuilder.Entity<FieldSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_field_source_mappings");

            entity.ToTable("field_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_field_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.FieldId).HasColumnName("field_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceRecordId)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("source_record_id");
            entity.Property(e => e.SourceRecordUrl)
                .HasMaxLength(1000)
                .HasColumnName("source_record_url");
            entity.Property(e => e.SourceSpecificData).HasColumnName("source_specific_data");

            entity.HasOne(d => d.Field).WithMany(p => p.FieldSourceMappings)
                .HasForeignKey(d => d.FieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_field_source_mappings_fields");
        });

        modelBuilder.Entity<Journal>(entity =>
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
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
            entity.Property(e => e.JournalTypeId).HasColumnName("journal_type_id");
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
            entity.Property(e => e.TwoYearMeanCitedness).HasColumnName("two_year_mean_citedness");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorksCount).HasColumnName("works_count");

            entity.HasOne(d => d.JournalTypeNavigation).WithMany(p => p.Journals)
                .HasForeignKey(d => d.JournalTypeId)
                .HasConstraintName("FK_core_journals_journal_types");
        });

        modelBuilder.Entity<JournalSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_journal_source_mappings");

            entity.ToTable("journal_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_journal_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.RawSourceId).HasColumnName("raw_source_id");
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
        });

        modelBuilder.Entity<JournalTopic>(entity =>
        {
            entity.HasKey(e => e.JournalTopicId).HasName("PK_core_journal_topics");

            entity.ToTable("journal_topics", "core");

            entity.HasIndex(e => new { e.JournalId, e.TopicId, e.SourceId }, "UQ_core_journal_topics_journal_topic_source").IsUnique();

            entity.Property(e => e.JournalTopicId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("journal_topic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.TopicShare)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("topic_share");
            entity.Property(e => e.WorksCount).HasColumnName("works_count");

            entity.HasOne(d => d.Journal).WithMany(p => p.JournalTopics)
                .HasForeignKey(d => d.JournalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_journal_topics_journals");

            entity.HasOne(d => d.Topic).WithMany(p => p.JournalTopics)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_journal_topics_topics");
        });

        modelBuilder.Entity<JournalType>(entity =>
        {
            entity.HasKey(e => e.JournalTypeId).HasName("PK_core_journal_types");

            entity.ToTable("journal_types", "core");

            entity.HasIndex(e => e.TypeCode, "UQ_core_journal_types_type_code").IsUnique();

            entity.Property(e => e.JournalTypeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("journal_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(255)
                .HasColumnName("display_name");
            entity.Property(e => e.TypeCode)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("type_code");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.KeywordId).HasName("PK_core_keywords");

            entity.ToTable("keywords", "core");

            entity.HasIndex(e => e.NormalizedName, "UQ_core_keywords_normalized_name").IsUnique();

            entity.Property(e => e.KeywordId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("keyword_id");
            entity.Property(e => e.CitedByCount).HasColumnName("cited_by_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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

        modelBuilder.Entity<KeywordSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_keyword_source_mappings");

            entity.ToTable("keyword_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_keyword_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
        });

        modelBuilder.Entity<Paper>(entity =>
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
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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

        modelBuilder.Entity<PaperAuthor>(entity =>
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
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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

        modelBuilder.Entity<PaperKeyword>(entity =>
        {
            entity.HasKey(e => e.PaperKeywordId).HasName("PK_core_paper_keywords");

            entity.ToTable("paper_keywords", "core");

            entity.HasIndex(e => new { e.PaperId, e.KeywordId, e.SourceId }, "UQ_core_paper_keywords_paper_keyword_source").IsUnique();

            entity.Property(e => e.PaperKeywordId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("paper_keyword_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
        });

        modelBuilder.Entity<PaperSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_paper_source_mappings");

            entity.ToTable("paper_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_paper_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
        });

        modelBuilder.Entity<PaperTopic>(entity =>
        {
            entity.HasKey(e => e.PaperTopicId).HasName("PK_core_paper_topics");

            entity.ToTable("paper_topics", "core");

            entity.HasIndex(e => new { e.PaperId, e.TopicId, e.SourceId }, "UQ_core_paper_topics_paper_topic_source").IsUnique();

            entity.Property(e => e.PaperTopicId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("paper_topic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.PaperId).HasColumnName("paper_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(10, 6)")
                .HasColumnName("score");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperTopics)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_topics_papers");

            entity.HasOne(d => d.Topic).WithMany(p => p.PaperTopics)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_paper_topics_topics");
        });

        modelBuilder.Entity<ResearchDomain>(entity =>
        {
            entity.HasKey(e => e.DomainId).HasName("PK_core_research_domains");

            entity.ToTable("research_domains", "core");

            entity.HasIndex(e => e.NormalizedName, "UQ_core_research_domains_normalized_name").IsUnique();

            entity.Property(e => e.DomainId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("domain_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.DomainName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("domain_name");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(255)
                .HasComputedColumnSql("(lower(ltrim(rtrim([domain_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ResearchField>(entity =>
        {
            entity.HasKey(e => e.FieldId).HasName("PK_core_research_fields");

            entity.ToTable("research_fields", "core");

            entity.HasIndex(e => new { e.DomainId, e.NormalizedName }, "UQ_core_research_fields_domain_name").IsUnique();

            entity.Property(e => e.FieldId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("field_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.DomainId).HasColumnName("domain_id");
            entity.Property(e => e.FieldName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("field_name");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(255)
                .HasComputedColumnSql("(lower(ltrim(rtrim([field_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Domain).WithMany(p => p.ResearchFields)
                .HasForeignKey(d => d.DomainId)
                .HasConstraintName("FK_core_research_fields_domains");
        });

        modelBuilder.Entity<ResearchSubfield>(entity =>
        {
            entity.HasKey(e => e.SubfieldId).HasName("PK_core_research_subfields");

            entity.ToTable("research_subfields", "core");

            entity.HasIndex(e => new { e.FieldId, e.NormalizedName }, "UQ_core_research_subfields_field_name").IsUnique();

            entity.Property(e => e.SubfieldId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("subfield_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.FieldId).HasColumnName("field_id");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(255)
                .HasComputedColumnSql("(lower(ltrim(rtrim([subfield_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.SubfieldName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("subfield_name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Field).WithMany(p => p.ResearchSubfields)
                .HasForeignKey(d => d.FieldId)
                .HasConstraintName("FK_core_research_subfields_fields");
        });

        modelBuilder.Entity<ResearchTopic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK_core_research_topics");

            entity.ToTable("research_topics", "core");

            entity.HasIndex(e => new { e.SubfieldId, e.NormalizedName }, "UQ_core_research_topics_subfield_name").IsUnique();

            entity.Property(e => e.TopicId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("topic_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
                .HasColumnName("created_at");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(500)
                .HasComputedColumnSql("(lower(ltrim(rtrim([topic_name]))))", true)
                .HasColumnName("normalized_name");
            entity.Property(e => e.SubfieldId).HasColumnName("subfield_id");
            entity.Property(e => e.TopicName)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("topic_name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Subfield).WithMany(p => p.ResearchTopics)
                .HasForeignKey(d => d.SubfieldId)
                .HasConstraintName("FK_core_research_topics_subfields");
        });

        modelBuilder.Entity<SubfieldSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_subfield_source_mappings");

            entity.ToTable("subfield_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_subfield_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
            entity.Property(e => e.SubfieldId).HasColumnName("subfield_id");

            entity.HasOne(d => d.Subfield).WithMany(p => p.SubfieldSourceMappings)
                .HasForeignKey(d => d.SubfieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_subfield_source_mappings_subfields");
        });

        modelBuilder.Entity<TopicSourceMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("PK_core_topic_source_mappings");

            entity.ToTable("topic_source_mappings", "core");

            entity.HasIndex(e => new { e.SourceId, e.SourceRecordId }, "UQ_core_topic_source_mappings_source_record").IsUnique();

            entity.Property(e => e.MappingId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(dateadd(hour,(7),sysutcdatetime()))")
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
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.Topic).WithMany(p => p.TopicSourceMappings)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_topic_source_mappings_topics");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_core_roles");

            entity.ToTable("roles", "core");

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
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
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnName("password");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(50)
                .HasColumnName("phonenumber");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_core_users_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
