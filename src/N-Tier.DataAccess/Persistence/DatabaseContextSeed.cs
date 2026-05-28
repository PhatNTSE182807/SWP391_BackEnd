using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.Shared.Helpers;

namespace N_Tier.DataAccess.Persistence;

public static class DatabaseContextSeed
{
    public static async Task SeedDatabaseAsync(DatabaseContext context)
    {
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
}
