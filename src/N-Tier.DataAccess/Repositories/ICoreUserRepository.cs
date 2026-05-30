using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Core.Entities;

namespace N_Tier.DataAccess.Repositories;

public interface ICoreUserRepository : IBaseRepository<User>
{
    Task<User> GetUserWithRoleByIdentifierAsync(string identifier);
    Task<Role> GetDefaultRoleAsync(string roleName);
    Task<User> InsertAsync(User user);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsPhoneExistsAsync(string phoneNumber);

    /// <summary>
    /// Lấy tất cả users kèm thông tin role (dùng cho Admin User Management)
    /// </summary>
    Task<List<User>> GetAllUsersWithRoleAsync();

    /// <summary>
    /// Lấy user theo UserId kèm thông tin role
    /// </summary>
    Task<User> GetUserByIdAsync(Guid userId);
}
