using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace N_Tier.DataAccess.Persistence;

public static class AutomatedMigration
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<DatabaseContext>();

        if (context.Database.IsSqlServer())
        {
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    await context.Database.MigrateAsync();
                }
                catch (Exception)
                {
                    // Ignore if no migration files exist in assembly
                }

                try
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('core.users') AND name = 'fcm_token') " +
                        "BEGIN ALTER TABLE core.users ADD fcm_token NVARCHAR(512) NULL; END"
                    );

                    await context.Database.ExecuteSqlRawAsync(
                        "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('core.notifications') AND type in (N'U')) " +
                        "BEGIN " +
                        "CREATE TABLE core.notifications ( " +
                        "    notification_id UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()), " +
                        "    user_id UNIQUEIDENTIFIER NOT NULL, " +
                        "    title NVARCHAR(500) NOT NULL, " +
                        "    body NVARCHAR(2000) NULL, " +
                        "    event_type NVARCHAR(100) NULL, " +
                        "    paper_id UNIQUEIDENTIFIER NULL, " +
                        "    topic_id UNIQUEIDENTIFIER NULL, " +
                        "    journal_id UNIQUEIDENTIFIER NULL, " +
                        "    is_read BIT NOT NULL DEFAULT 0, " +
                        "    created_at DATETIME2 NOT NULL DEFAULT (DATEADD(hour, (7), SYSUTCDATETIME())), " +
                        "    CONSTRAINT PK_core_notifications PRIMARY KEY (notification_id), " +
                        "    CONSTRAINT FK_core_notifications_users FOREIGN KEY (user_id) REFERENCES core.users (user_id) ON DELETE CASCADE, " +
                        "    CONSTRAINT FK_core_notifications_papers FOREIGN KEY (paper_id) REFERENCES core.papers (paper_id) ON DELETE SET NULL " +
                        "); " +
                        "END"
                    );
                }
                catch (Exception)
                {
                    // Ignore or log.
                }
            });
        }

        try
        {
            await DatabaseContextSeed.SeedDatabaseAsync(context);
        }
        catch (Exception)
        {
            // Ignore seed exceptions if data already present
        }
    }
}
