-- =============================================
-- Hangfire Schema Creation Script
-- Run this script ONCE to create Hangfire tables
-- =============================================

USE [scientific_journal_tracking_db];
GO

-- Create HangFire schema if not exists
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'HangFire')
BEGIN
    EXEC('CREATE SCHEMA [HangFire]');
END
GO

-- Grant permissions to backend_user
GRANT CREATE TABLE TO backend_user;
GRANT ALTER ON SCHEMA::HangFire TO backend_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::HangFire TO backend_user;
GO

-- Create HangFire.Schema table (version tracking)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Schema]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[Schema](
        [Version] [int] NOT NULL,
        CONSTRAINT [PK_HangFire_Schema] PRIMARY KEY CLUSTERED ([Version] ASC)
    );
END
GO

-- Create HangFire.Set table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Set]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[Set](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Key] [nvarchar](100) NOT NULL,
        [Score] [float] NOT NULL,
        [Value] [nvarchar](256) NOT NULL,
        [ExpireAt] [datetime] NULL,
        CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_Set_Key_Score] ON [HangFire].[Set] ([Key] ASC, [Score] ASC);
END
GO

-- Create HangFire.Hash table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Hash]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[Hash](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Key] [nvarchar](100) NOT NULL,
        [Field] [nvarchar](100) NOT NULL,
        [Value] [nvarchar](max) NULL,
        [ExpireAt] [datetime2](7) NULL,
        CONSTRAINT [PK_HangFire_Hash] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE UNIQUE NONCLUSTERED INDEX [UX_HangFire_Hash_Key_Field] ON [HangFire].[Hash] ([Key] ASC, [Field] ASC);
END
GO

-- Create HangFire.Job table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Job]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[Job](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [StateId] [bigint] NULL,
        [StateName] [nvarchar](20) NULL,
        [InvocationData] [nvarchar](max) NOT NULL,
        [Arguments] [nvarchar](max) NOT NULL,
        [CreatedAt] [datetime] NOT NULL,
        [ExpireAt] [datetime] NULL,
        CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_Job_StateName] ON [HangFire].[Job] ([StateName] ASC);
END
GO

-- Create HangFire.State table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[State]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[State](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [JobId] [bigint] NOT NULL,
        [Name] [nvarchar](20) NOT NULL,
        [Reason] [nvarchar](100) NULL,
        [CreatedAt] [datetime] NOT NULL,
        [Data] [nvarchar](max) NULL,
        CONSTRAINT [PK_HangFire_State] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_State_JobId] ON [HangFire].[State] ([JobId] ASC);
    
    ALTER TABLE [HangFire].[State] WITH CHECK ADD CONSTRAINT [FK_HangFire_State_Job] 
        FOREIGN KEY([JobId]) REFERENCES [HangFire].[Job] ([Id]) ON UPDATE CASCADE ON DELETE CASCADE;
END
GO

-- Create HangFire.JobParameter table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[JobParameter]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[JobParameter](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [JobId] [bigint] NOT NULL,
        [Name] [nvarchar](40) NOT NULL,
        [Value] [nvarchar](max) NULL,
        CONSTRAINT [PK_HangFire_JobParameter] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_JobParameter_JobIdAndName] ON [HangFire].[JobParameter] ([JobId] ASC, [Name] ASC);
    
    ALTER TABLE [HangFire].[JobParameter] WITH CHECK ADD CONSTRAINT [FK_HangFire_JobParameter_Job] 
        FOREIGN KEY([JobId]) REFERENCES [HangFire].[Job] ([Id]) ON UPDATE CASCADE ON DELETE CASCADE;
END
GO

-- Create HangFire.JobQueue table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[JobQueue]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[JobQueue](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [JobId] [bigint] NOT NULL,
        [Queue] [nvarchar](50) NOT NULL,
        [FetchedAt] [datetime] NULL,
        CONSTRAINT [PK_HangFire_JobQueue] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_JobQueue_QueueAndFetchedAt] ON [HangFire].[JobQueue] ([Queue] ASC, [FetchedAt] ASC);
END
GO

-- Create HangFire.Server table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Server]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[Server](
        [Id] [nvarchar](200) NOT NULL,
        [Data] [nvarchar](max) NULL,
        [LastHeartbeat] [datetime] NOT NULL,
        CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Create HangFire.List table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[List]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[List](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Key] [nvarchar](100) NOT NULL,
        [Value] [nvarchar](max) NULL,
        [ExpireAt] [datetime] NULL,
        CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_List_Key] ON [HangFire].[List] ([Key] ASC);
END
GO

-- Create HangFire.Counter table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Counter]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[Counter](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Key] [nvarchar](100) NOT NULL,
        [Value] [int] NOT NULL,
        [ExpireAt] [datetime] NULL,
        CONSTRAINT [PK_HangFire_Counter] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE NONCLUSTERED INDEX [IX_HangFire_Counter_Key] ON [HangFire].[Counter] ([Key] ASC);
END
GO

-- Create HangFire.AggregatedCounter table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[AggregatedCounter]') AND type in (N'U'))
BEGIN
    CREATE TABLE [HangFire].[AggregatedCounter](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Key] [nvarchar](100) NOT NULL,
        [Value] [bigint] NOT NULL,
        [ExpireAt] [datetime] NULL,
        CONSTRAINT [PK_HangFire_CounterAggregated] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    CREATE UNIQUE NONCLUSTERED INDEX [UX_HangFire_CounterAggregated_Key] ON [HangFire].[AggregatedCounter] ([Key] ASC);
END
GO

-- Insert schema version
IF NOT EXISTS (SELECT * FROM [HangFire].[Schema] WHERE [Version] = 7)
BEGIN
    INSERT INTO [HangFire].[Schema] ([Version]) VALUES (7);
END
GO

PRINT 'Hangfire schema created successfully!';
GO
