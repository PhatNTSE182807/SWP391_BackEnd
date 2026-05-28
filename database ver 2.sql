CREATE DATABASE scientific_journal_tracking_db;
GO

USE scientific_journal_tracking_db;
GO

CREATE SCHEMA raw;
GO

CREATE SCHEMA core;
GO


-- =========================================================
-- raw.api_sources
-- Store academic API source list
-- =========================================================

CREATE TABLE raw.api_sources (
    source_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    source_name NVARCHAR(100) NOT NULL,
    base_url NVARCHAR(500) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_raw_api_sources
        PRIMARY KEY (source_id),

    CONSTRAINT UQ_raw_api_sources_source_name
        UNIQUE (source_name)
);
GO


-- =========================================================
-- raw.pipeline_runs
-- Store API sync logs
-- =========================================================

CREATE TABLE raw.pipeline_runs (
    run_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    source_id UNIQUEIDENTIFIER NOT NULL,
    source_entity NVARCHAR(100) NOT NULL DEFAULT 'works',
    query_keyword NVARCHAR(255) NULL,

    status NVARCHAR(50) NOT NULL DEFAULT 'running',
    started_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    finished_at DATETIME2 NULL,

    records_fetched INT NOT NULL DEFAULT 0,
    records_inserted INT NOT NULL DEFAULT 0,
    records_failed INT NOT NULL DEFAULT 0,
    error_message NVARCHAR(MAX) NULL,

    CONSTRAINT PK_raw_pipeline_runs
        PRIMARY KEY (run_id),

    CONSTRAINT FK_raw_pipeline_runs_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT CK_raw_pipeline_runs_status
        CHECK (status IN ('running', 'success', 'failed')),

    CONSTRAINT CK_raw_pipeline_runs_records_fetched
        CHECK (records_fetched >= 0),

    CONSTRAINT CK_raw_pipeline_runs_records_inserted
        CHECK (records_inserted >= 0),

    CONSTRAINT CK_raw_pipeline_runs_records_failed
        CHECK (records_failed >= 0)
);
GO


-- =========================================================
-- raw.works
-- Store raw API response of papers / works
-- =========================================================

CREATE TABLE raw.works (
    raw_work_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    source_id UNIQUEIDENTIFIER NOT NULL,
    source_entity NVARCHAR(100) NOT NULL DEFAULT 'works',
    source_record_id NVARCHAR(500) NOT NULL,

    query_keyword NVARCHAR(255) NULL,
    pipeline_run_id UNIQUEIDENTIFIER NOT NULL,

    raw_data NVARCHAR(MAX) NOT NULL,

    fetched_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    last_seen_at DATETIME2 NULL,
    processed_status NVARCHAR(50) NOT NULL DEFAULT 'pending',
    process_error NVARCHAR(MAX) NULL,

    CONSTRAINT PK_raw_works
        PRIMARY KEY (raw_work_id),

    CONSTRAINT FK_raw_works_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT FK_raw_works_pipeline_runs
        FOREIGN KEY (pipeline_run_id)
        REFERENCES raw.pipeline_runs(run_id),

    CONSTRAINT UQ_raw_works_source_record
        UNIQUE (source_id, source_record_id),

    CONSTRAINT CK_raw_works_raw_data_json
        CHECK (ISJSON(raw_data) = 1),

    CONSTRAINT CK_raw_works_processed_status
        CHECK (processed_status IN ('pending', 'processed', 'failed'))
);
GO


-- =========================================================
-- core.journals
-- Internal journal records. External ids such as OpenAlex S...
-- are stored in core.journal_source_mappings.source_record_id.
-- =========================================================

CREATE TABLE core.journals (
    journal_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    journal_name NVARCHAR(500) NOT NULL,
    normalized_name AS LOWER(LTRIM(RTRIM(journal_name))) PERSISTED,

    issn_l NVARCHAR(50) NULL,
    issn_print NVARCHAR(50) NULL,
    issn_electronic NVARCHAR(50) NULL,

    publisher NVARCHAR(500) NULL,
    host_organization_name NVARCHAR(500) NULL,

    journal_type NVARCHAR(100) NULL,
    homepage_url NVARCHAR(1000) NULL,
    country_code NVARCHAR(20) NULL,

    works_count INT NULL,
    cited_by_count INT NULL,
    oa_works_count INT NULL,

    h_index INT NULL,
    i10_index INT NULL,
    two_year_mean_citedness DECIMAL(10, 4) NULL,

    is_open_access BIT NULL,
    is_in_doaj BIT NULL,
    is_core BIT NULL,

    first_publication_year INT NULL,
    last_publication_year INT NULL,

    subjects NVARCHAR(MAX) NULL,
    topics NVARCHAR(MAX) NULL,
    counts_by_year NVARCHAR(MAX) NULL,

    source_created_date DATETIME2 NULL,
    source_updated_date DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,

    CONSTRAINT PK_core_journals
        PRIMARY KEY (journal_id),

    CONSTRAINT CK_core_journals_journal_type
        CHECK (
            journal_type IS NULL OR
            journal_type IN ('journal', 'repository', 'conference', 'book-series', 'ebook-platform', 'other')
        ),

    CONSTRAINT CK_core_journals_works_count
        CHECK (works_count IS NULL OR works_count >= 0),

    CONSTRAINT CK_core_journals_cited_by_count
        CHECK (cited_by_count IS NULL OR cited_by_count >= 0),

    CONSTRAINT CK_core_journals_oa_works_count
        CHECK (oa_works_count IS NULL OR oa_works_count >= 0),

    CONSTRAINT CK_core_journals_h_index
        CHECK (h_index IS NULL OR h_index >= 0),

    CONSTRAINT CK_core_journals_i10_index
        CHECK (i10_index IS NULL OR i10_index >= 0),

    CONSTRAINT CK_core_journals_first_publication_year
        CHECK (
            first_publication_year IS NULL
            OR first_publication_year BETWEEN 1500 AND 2100
        ),

    CONSTRAINT CK_core_journals_last_publication_year
        CHECK (
            last_publication_year IS NULL
            OR last_publication_year BETWEEN 1500 AND 2100
        ),

    CONSTRAINT CK_core_journals_subjects_json
        CHECK (subjects IS NULL OR ISJSON(subjects) = 1),

    CONSTRAINT CK_core_journals_topics_json
        CHECK (topics IS NULL OR ISJSON(topics) = 1),

    CONSTRAINT CK_core_journals_counts_by_year_json
        CHECK (counts_by_year IS NULL OR ISJSON(counts_by_year) = 1)
);
GO


-- =========================================================
-- core.journal_source_mappings
-- Map internal journal_id to external source ids, e.g. OpenAlex S...
-- =========================================================

CREATE TABLE core.journal_source_mappings (
    mapping_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    journal_id UNIQUEIDENTIFIER NOT NULL,
    source_id UNIQUEIDENTIFIER NOT NULL,

    source_record_id NVARCHAR(500) NOT NULL,
    source_record_url NVARCHAR(1000) NULL,

    source_specific_data NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_core_journal_source_mappings
        PRIMARY KEY (mapping_id),

    CONSTRAINT FK_core_journal_source_mappings_journals
        FOREIGN KEY (journal_id)
        REFERENCES core.journals(journal_id),

    CONSTRAINT FK_core_journal_source_mappings_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT UQ_core_journal_source_mappings_source_record
        UNIQUE (source_id, source_record_id),

    CONSTRAINT CK_core_journal_source_mappings_source_specific_data_json
        CHECK (
            source_specific_data IS NULL
            OR ISJSON(source_specific_data) = 1
        )
);
GO


-- =========================================================
-- core.papers
-- Internal paper records. External ids such as OpenAlex W...
-- are stored in core.paper_source_mappings.source_record_id.
-- =========================================================

CREATE TABLE core.papers (
    paper_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    doi NVARCHAR(500) NULL,

    title NVARCHAR(2000) NOT NULL,
    abstract NVARCHAR(MAX) NULL,

    publication_year INT NULL,
    publication_date DATE NULL,

    paper_type NVARCHAR(100) NULL,
    language NVARCHAR(20) NULL,

    cited_by_count INT NULL,
    reference_count INT NULL,

    volume NVARCHAR(50) NULL,
    issue NVARCHAR(50) NULL,
    page NVARCHAR(100) NULL,

    is_open_access BIT NULL,
    is_retracted BIT NULL,

    journal_id UNIQUEIDENTIFIER NULL,

    referenced_works NVARCHAR(MAX) NULL,
    related_works NVARCHAR(MAX) NULL,
    abstract_inverted_index NVARCHAR(MAX) NULL,
    counts_by_year NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,

    CONSTRAINT PK_core_papers
        PRIMARY KEY (paper_id),

    CONSTRAINT FK_core_papers_journals
        FOREIGN KEY (journal_id)
        REFERENCES core.journals(journal_id),

    CONSTRAINT CK_core_papers_publication_year
        CHECK (
            publication_year IS NULL
            OR publication_year BETWEEN 1500 AND 2100
        ),

    CONSTRAINT CK_core_papers_cited_by_count
        CHECK (
            cited_by_count IS NULL
            OR cited_by_count >= 0
        ),

    CONSTRAINT CK_core_papers_reference_count
        CHECK (
            reference_count IS NULL
            OR reference_count >= 0
        ),

    CONSTRAINT CK_core_papers_referenced_works_json
        CHECK (referenced_works IS NULL OR ISJSON(referenced_works) = 1),

    CONSTRAINT CK_core_papers_related_works_json
        CHECK (related_works IS NULL OR ISJSON(related_works) = 1),

    CONSTRAINT CK_core_papers_abstract_inverted_index_json
        CHECK (abstract_inverted_index IS NULL OR ISJSON(abstract_inverted_index) = 1),

    CONSTRAINT CK_core_papers_counts_by_year_json
        CHECK (counts_by_year IS NULL OR ISJSON(counts_by_year) = 1)
);
GO


-- =========================================================
-- core.paper_source_mappings
-- Map internal paper_id to external source ids, e.g. OpenAlex W...
-- =========================================================

CREATE TABLE core.paper_source_mappings (
    mapping_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    paper_id UNIQUEIDENTIFIER NOT NULL,
    source_id UNIQUEIDENTIFIER NOT NULL,
    raw_work_id UNIQUEIDENTIFIER NULL,

    source_record_id NVARCHAR(500) NOT NULL,
    source_record_url NVARCHAR(1000) NULL,
    source_specific_data NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_core_paper_source_mappings
        PRIMARY KEY (mapping_id),

    CONSTRAINT FK_core_paper_source_mappings_papers
        FOREIGN KEY (paper_id)
        REFERENCES core.papers(paper_id),

    CONSTRAINT FK_core_paper_source_mappings_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT FK_core_paper_source_mappings_raw_works
        FOREIGN KEY (raw_work_id)
        REFERENCES raw.works(raw_work_id),

    CONSTRAINT UQ_core_paper_source_mappings_source_record
        UNIQUE (source_id, source_record_id),

    CONSTRAINT CK_core_paper_source_mappings_source_specific_data_json
        CHECK (
            source_specific_data IS NULL
            OR ISJSON(source_specific_data) = 1
        )
);
GO


-- =========================================================
-- core.authors
-- Internal author records. External ids such as OpenAlex A...
-- are stored in core.author_source_mappings.source_record_id.
-- =========================================================

CREATE TABLE core.authors (
    author_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    display_name NVARCHAR(500) NOT NULL,
    normalized_name AS LOWER(LTRIM(RTRIM(display_name))) PERSISTED,

    full_name NVARCHAR(500) NULL,
    orcid NVARCHAR(100) NULL,

    works_count INT NULL,
    cited_by_count INT NULL,

    h_index INT NULL,
    i10_index INT NULL,
    two_year_mean_citedness DECIMAL(10, 4) NULL,

    raw_author_names NVARCHAR(MAX) NULL,
    display_name_alternatives NVARCHAR(MAX) NULL,

    affiliations NVARCHAR(MAX) NULL,
    last_known_institutions NVARCHAR(MAX) NULL,

    topics NVARCHAR(MAX) NULL,
    topic_share NVARCHAR(MAX) NULL,
    x_concepts NVARCHAR(MAX) NULL,
    counts_by_year NVARCHAR(MAX) NULL,

    works_api_url NVARCHAR(1000) NULL,

    source_created_date DATETIME2 NULL,
    source_updated_date DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,

    CONSTRAINT PK_core_authors
        PRIMARY KEY (author_id),

    CONSTRAINT CK_core_authors_raw_author_names_json
        CHECK (raw_author_names IS NULL OR ISJSON(raw_author_names) = 1),

    CONSTRAINT CK_core_authors_display_name_alternatives_json
        CHECK (display_name_alternatives IS NULL OR ISJSON(display_name_alternatives) = 1),

    CONSTRAINT CK_core_authors_affiliations_json
        CHECK (affiliations IS NULL OR ISJSON(affiliations) = 1),

    CONSTRAINT CK_core_authors_last_known_institutions_json
        CHECK (last_known_institutions IS NULL OR ISJSON(last_known_institutions) = 1),

    CONSTRAINT CK_core_authors_topics_json
        CHECK (topics IS NULL OR ISJSON(topics) = 1),

    CONSTRAINT CK_core_authors_topic_share_json
        CHECK (topic_share IS NULL OR ISJSON(topic_share) = 1),

    CONSTRAINT CK_core_authors_x_concepts_json
        CHECK (x_concepts IS NULL OR ISJSON(x_concepts) = 1),

    CONSTRAINT CK_core_authors_counts_by_year_json
        CHECK (counts_by_year IS NULL OR ISJSON(counts_by_year) = 1)
);
GO


-- =========================================================
-- core.paper_authors
-- Link papers to authors
-- =========================================================

CREATE TABLE core.paper_authors (
    paper_author_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    paper_id UNIQUEIDENTIFIER NOT NULL,
    author_id UNIQUEIDENTIFIER NOT NULL,

    author_order INT NULL,
    author_position NVARCHAR(50) NULL,
    raw_author_name NVARCHAR(500) NULL,
    is_corresponding BIT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_core_paper_authors
        PRIMARY KEY (paper_author_id),

    CONSTRAINT FK_core_paper_authors_papers
        FOREIGN KEY (paper_id)
        REFERENCES core.papers(paper_id),

    CONSTRAINT FK_core_paper_authors_authors
        FOREIGN KEY (author_id)
        REFERENCES core.authors(author_id),

    CONSTRAINT UQ_core_paper_authors_paper_author
        UNIQUE (paper_id, author_id),

    CONSTRAINT CK_core_paper_authors_author_order
        CHECK (author_order IS NULL OR author_order > 0),

    CONSTRAINT CK_core_paper_authors_author_position
        CHECK (
            author_position IS NULL OR
            author_position IN ('first', 'middle', 'last', 'additional')
        )
);
GO


-- =========================================================
-- core.author_source_mappings
-- Map internal author_id to external source ids, e.g. OpenAlex A...
-- =========================================================

CREATE TABLE core.author_source_mappings (
    mapping_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    author_id UNIQUEIDENTIFIER NOT NULL,
    source_id UNIQUEIDENTIFIER NOT NULL,

    source_record_id NVARCHAR(500) NOT NULL,
    source_record_url NVARCHAR(1000) NULL,
    source_specific_data NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_core_author_source_mappings
        PRIMARY KEY (mapping_id),

    CONSTRAINT FK_core_author_source_mappings_authors
        FOREIGN KEY (author_id)
        REFERENCES core.authors(author_id),

    CONSTRAINT FK_core_author_source_mappings_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT UQ_core_author_source_mappings_source_record
        UNIQUE (source_id, source_record_id),

    CONSTRAINT CK_core_author_source_mappings_source_specific_data_json
        CHECK (
            source_specific_data IS NULL
            OR ISJSON(source_specific_data) = 1
        )
);
GO


-- =========================================================
-- core.keywords
-- Internal keyword records. External ids are stored in
-- core.keyword_source_mappings.source_record_id.
-- =========================================================

CREATE TABLE core.keywords (
    keyword_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    keyword_name NVARCHAR(255) NOT NULL,
    normalized_name AS LOWER(LTRIM(RTRIM(keyword_name))) PERSISTED,

    works_count INT NULL,
    cited_by_count INT NULL,
    works_api_url NVARCHAR(1000) NULL,

    source_created_date DATE NULL,
    source_updated_date DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,

    CONSTRAINT PK_core_keywords
        PRIMARY KEY (keyword_id),

    CONSTRAINT UQ_core_keywords_normalized_name
        UNIQUE (normalized_name),

    CONSTRAINT CK_core_keywords_works_count
        CHECK (works_count IS NULL OR works_count >= 0),

    CONSTRAINT CK_core_keywords_cited_by_count
        CHECK (cited_by_count IS NULL OR cited_by_count >= 0)
);
GO


-- =========================================================
-- core.paper_keywords
-- Link papers to keywords
-- =========================================================

CREATE TABLE core.paper_keywords (
    paper_keyword_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    paper_id UNIQUEIDENTIFIER NOT NULL,
    keyword_id UNIQUEIDENTIFIER NOT NULL,

    score DECIMAL(10, 6) NULL,
    source_id UNIQUEIDENTIFIER NOT NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_core_paper_keywords
        PRIMARY KEY (paper_keyword_id),

    CONSTRAINT FK_core_paper_keywords_papers
        FOREIGN KEY (paper_id)
        REFERENCES core.papers(paper_id),

    CONSTRAINT FK_core_paper_keywords_keywords
        FOREIGN KEY (keyword_id)
        REFERENCES core.keywords(keyword_id),

    CONSTRAINT FK_core_paper_keywords_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT UQ_core_paper_keywords_paper_keyword_source
        UNIQUE (paper_id, keyword_id, source_id),

    CONSTRAINT CK_core_paper_keywords_score
        CHECK (
            score IS NULL
            OR score BETWEEN 0 AND 1
        )
);
GO


-- =========================================================
-- core.keyword_source_mappings
-- Map internal keyword_id to external source ids
-- =========================================================

CREATE TABLE core.keyword_source_mappings (
    mapping_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),

    keyword_id UNIQUEIDENTIFIER NOT NULL,
    source_id UNIQUEIDENTIFIER NOT NULL,

    source_record_id NVARCHAR(500) NOT NULL,
    source_record_url NVARCHAR(1000) NULL,
    source_specific_data NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_core_keyword_source_mappings
        PRIMARY KEY (mapping_id),

    CONSTRAINT FK_core_keyword_source_mappings_keywords
        FOREIGN KEY (keyword_id)
        REFERENCES core.keywords(keyword_id),

    CONSTRAINT FK_core_keyword_source_mappings_api_sources
        FOREIGN KEY (source_id)
        REFERENCES raw.api_sources(source_id),

    CONSTRAINT UQ_core_keyword_source_mappings_source_record
        UNIQUE (source_id, source_record_id),

    CONSTRAINT CK_core_keyword_source_mappings_source_specific_data_json
        CHECK (
            source_specific_data IS NULL
            OR ISJSON(source_specific_data) = 1
        )
);
GO


-- =========================================================
-- Seed default source
-- =========================================================

INSERT INTO raw.api_sources (source_name, base_url)
VALUES ('OpenAlex', 'https://api.openalex.org');
GO


-- =========================================================
-- core.roles
-- =========================================================

CREATE TABLE core.roles (
    role_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    role_name NVARCHAR(100) NOT NULL,

    CONSTRAINT PK_core_roles
        PRIMARY KEY (role_id),

    CONSTRAINT UQ_core_roles_role_name
        UNIQUE (role_name)
);
GO


-- =========================================================
-- core.users
-- =========================================================

CREATE TABLE core.users (
    user_id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    username NVARCHAR(100) NOT NULL,
    email NVARCHAR(256) NOT NULL,
    password NVARCHAR(256) NOT NULL,
    role_id UNIQUEIDENTIFIER NOT NULL,
    phonenumber NVARCHAR(50) NULL,

    CONSTRAINT PK_core_users
        PRIMARY KEY (user_id),

    CONSTRAINT UQ_core_users_username
        UNIQUE (username),

    CONSTRAINT UQ_core_users_email
        UNIQUE (email),

    CONSTRAINT FK_core_users_roles
        FOREIGN KEY (role_id)
        REFERENCES core.roles(role_id)
);
GO


-- Seed roles
INSERT INTO core.roles (role_name)
VALUES 
    ('System Administrator'),
    ('Researcher'),
    ('Lecturer'),
    ('Student');
GO
