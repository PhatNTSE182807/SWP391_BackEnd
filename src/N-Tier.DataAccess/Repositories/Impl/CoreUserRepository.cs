using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.DataAccess.Repositories.Impl;

public class CoreUserRepository : BaseRepository<User>, ICoreUserRepository
{
    public CoreUserRepository(DatabaseContext context) : base(context) { }

    public async Task<User> GetUserWithRoleByIdentifierAsync(string identifier)
    {
        return await DbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier || u.Phonenumber == identifier);
    }

    public async Task<Role> GetDefaultRoleAsync(string roleName)
    {
        return await Context.CoreRoles
            .FirstOrDefaultAsync(r => r.RoleName == roleName);
    }

    public async Task<User> InsertAsync(User user)
    {
        var addedUser = (await DbSet.AddAsync(user)).Entity;
        await Context.SaveChangesAsync();
        return addedUser;
    }

    public async Task<bool> IsUsernameExistsAsync(string username)
        => await DbSet.AnyAsync(u => u.Username == username);

    public async Task<bool> IsEmailExistsAsync(string email)
        => await DbSet.AnyAsync(u => u.Email == email);

    public async Task<bool> IsPhoneExistsAsync(string phoneNumber)
        => await DbSet.AnyAsync(u => u.Phonenumber == phoneNumber);
}
