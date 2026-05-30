using System;
using System.Collections.Generic;
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

    public async Task<bool> IsUsernameExistsExceptAsync(string username, Guid excludeUserId)
        => await DbSet.AnyAsync(u => u.Username == username && u.UserId != excludeUserId);

    public async Task<bool> IsEmailExistsExceptAsync(string email, Guid excludeUserId)
        => await DbSet.AnyAsync(u => u.Email == email && u.UserId != excludeUserId);

    public async Task<bool> IsPhoneExistsExceptAsync(string phoneNumber, Guid excludeUserId)
        => await DbSet.AnyAsync(u => u.Phonenumber == phoneNumber && u.UserId != excludeUserId);

    public async Task<List<User>> GetAllUsersWithRoleAsync()
    {
        return await DbSet
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await DbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }
}
