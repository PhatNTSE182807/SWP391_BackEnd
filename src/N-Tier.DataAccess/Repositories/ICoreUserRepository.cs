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
}
