using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.Shared.Helpers;

namespace N_Tier.DataAccess.Persistence;

public static class DatabaseContextSeed
{
    public static async Task SeedDatabaseAsync(DatabaseContext context)
    {
        if (!await IsTableExistsAsync(context, "core", "users") ||
            !await IsTableExistsAsync(context, "core", "roles"))
        {
            return;
        }

        if (!await context.CoreUsers.AnyAsync())
        {
            var role = await context.CoreRoles.FirstOrDefaultAsync(r => r.RoleName == "System Administrator");
            if (role != null)
            {
                var customUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "system_admin",
                    Email = "admin@journal.com",
                    Password = PasswordHasher.HashPassword("Admin123!?"),
                    RoleId = role.RoleId,
                    Phonenumber = "0987654321"
                };
                await context.CoreUsers.AddAsync(customUser);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<bool> IsTableExistsAsync(DatabaseContext context, string schemaName, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = @schemaName
  AND TABLE_NAME = @tableName";

            var schemaParameter = command.CreateParameter();
            schemaParameter.ParameterName = "@schemaName";
            schemaParameter.Value = schemaName;
            command.Parameters.Add(schemaParameter);

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var result = await command.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
}
