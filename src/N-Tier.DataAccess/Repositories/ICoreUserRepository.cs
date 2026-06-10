using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface ICoreUserRepository : IBaseRepository<User>
{
    Task<User> GetUserWithRoleByIdentifierAsync(string identifier);
    Task<User> GetUserWithRoleByEmailAsync(string email);
    Task<Role> GetDefaultRoleAsync(string roleName);
    Task<User> InsertAsync(User user);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsPhoneExistsAsync(string phoneNumber);
    
    Task<bool> IsUsernameExistsExceptAsync(string username, Guid excludeUserId);
    Task<bool> IsEmailExistsExceptAsync(string email, Guid excludeUserId);
    Task<bool> IsPhoneExistsExceptAsync(string phoneNumber, Guid excludeUserId);

    Task<List<User>> GetAllUsersWithRoleAsync();
    Task<User> GetUserByIdAsync(Guid userId);
}
