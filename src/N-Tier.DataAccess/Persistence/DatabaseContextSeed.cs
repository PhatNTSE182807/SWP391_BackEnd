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

        // Ensure roles exist
        var rolesToSeed = new[] { "System Administrator", "Researcher", "Lecturer", "Student" };
        foreach (var roleName in rolesToSeed)
        {
            var existingRole = await context.CoreRoles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (existingRole == null)
            {
                await context.CoreRoles.AddAsync(new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = roleName
                });
            }
        }
        await context.SaveChangesAsync();

        // Local helper method to seed a user
        async Task SeedUserAsync(string username, string email, string password, string roleName, string phone)
        {
            var role = await context.CoreRoles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null) return;

            var user = await context.CoreUsers.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = username,
                    Email = email,
                    Password = PasswordHasher.HashPassword(password),
                    RoleId = role.RoleId,
                    Phonenumber = phone,
                    IsActive = true
                };
                await context.CoreUsers.AddAsync(newUser);
            }
            else
            {
                user.Email = email;
                user.Password = PasswordHasher.HashPassword(password);
                user.RoleId = role.RoleId;
                context.CoreUsers.Update(user);
            }
        }

        // Seed users
        await SeedUserAsync("system_admin", "admin@cloud.com", "Admin123@", "System Administrator", "0987654321");
        await SeedUserAsync("researcher_user", "researcher@cloud.com", "Password123@", "Researcher", "0987654322");
        await SeedUserAsync("lecturer_user", "lecturer@cloud.com", "Password123@", "Lecturer", "0987654323");
        await SeedUserAsync("student_user", "student@cloud.com", "Password123@", "Student", "0987654324");

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
