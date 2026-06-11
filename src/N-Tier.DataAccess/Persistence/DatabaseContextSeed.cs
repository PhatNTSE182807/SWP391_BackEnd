using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.Shared.Helpers;

namespace N_Tier.DataAccess.Persistence;

public static class DatabaseContextSeed
{
    public static async Task SeedDatabaseAsync(DatabaseContext context)
    {
        if (!await IsTableExistsAsync(context, "core", "roles"))
            return;

        // Seed roles nếu chưa có
        await SeedRolesAsync(context);

        if (!await IsTableExistsAsync(context, "core", "users"))
            return;

        // Seed admin users
        await SeedAdminUsersAsync(context);
    }

    private static async Task SeedRolesAsync(DatabaseContext context)
    {
        var roles = new[] { "System Administrator", "Researcher", "Lecturer", "Student" };

        foreach (var roleName in roles)
        {
            if (!await context.CoreRoles.AnyAsync(r => r.RoleName == roleName))
            {
                await context.CoreRoles.AddAsync(new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = roleName
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUsersAsync(DatabaseContext context)
    {
        var role = await context.CoreRoles
            .FirstOrDefaultAsync(r => r.RoleName == "System Administrator");

        if (role == null)
            return;

        
        if (!await context.CoreUsers.AnyAsync(u => u.Username == "system_admin" || u.Email == "admin@journal.com"))
        {
            await context.CoreUsers.AddAsync(new User
            {
                UserId = Guid.NewGuid(),
                Username = "system_admin",
                Email = "admin@journal.com",
                Password = PasswordHasher.HashPassword("Admin123!?"),
                RoleId = role.RoleId,
                Phonenumber = "0987654321"
            });
        }

        if (!await context.CoreUsers.AnyAsync(u => u.Username == "admin2" || u.Email == "admin@gmail.com"))
        {
            await context.CoreUsers.AddAsync(new User
            {
                UserId = Guid.NewGuid(),
                Username = "admin2",
                Email = "admin@gmail.com",
                Password = PasswordHasher.HashPassword("Admin123!?"),
                RoleId = role.RoleId,
                Phonenumber = "0987654322"
            });
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
