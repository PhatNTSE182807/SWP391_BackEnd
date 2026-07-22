using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
            await context.Database.MigrateAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('core.users') AND name = 'fcm_token') " +
                    "BEGIN ALTER TABLE core.users ADD fcm_token NVARCHAR(512) NULL; END"
                );
            }
            catch (Exception)
            {
                // Ignore or log.
            }
        }

        await DatabaseContextSeed.SeedDatabaseAsync(context);
    }
}
