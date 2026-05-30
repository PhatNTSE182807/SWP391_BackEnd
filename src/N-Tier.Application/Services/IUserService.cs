using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.User;

namespace N_Tier.Application.Services;

public interface IUserService
{
    /// <summary>
    /// Lấy danh sách tất cả users kèm thông tin role và trạng thái
    /// </summary>
    Task<List<UserResponseModel>> GetAllUsersAsync();

    /// <summary>
    /// Toggle trạng thái active/deactivated của user.
    /// Admin không thể deactivate chính mình.
    /// </summary>
    Task<UserResponseModel> ToggleDeactivateUserAsync(Guid userId);

    /// <summary>
    /// Xóa tài khoản user theo userId.
    /// Admin không thể tự xóa chính mình.
    /// </summary>
    Task DeleteUserAsync(Guid userId);
}
