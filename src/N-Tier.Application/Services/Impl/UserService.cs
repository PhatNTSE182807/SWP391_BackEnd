using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using N_Tier.Application.Exceptions;
using N_Tier.Application.Models.User;
using N_Tier.DataAccess.Repositories;
using N_Tier.Shared.Helpers;
using N_Tier.Shared.Services;

namespace N_Tier.Application.Services.Impl;

public class UserService : IUserService
{
    private readonly ICoreUserRepository _coreUserRepository;
    private readonly IClaimService _claimService;

    public UserService(ICoreUserRepository coreUserRepository, IClaimService claimService)
    {
        _coreUserRepository = coreUserRepository;
        _claimService = claimService;
    }

    public async Task<List<UserResponseModel>> GetAllUsersAsync()
    {
        var users = await _coreUserRepository.GetAllUsersWithRoleAsync();

        return users.Select(u => new UserResponseModel
        {
            UserId      = u.UserId,
            Username    = u.Username,
            Email       = u.Email,
            Phonenumber = u.Phonenumber,
            RoleName    = u.Role?.RoleName,
            IsActive    = u.IsActive
        }).ToList();
    }

    public async Task<UserResponseModel> ToggleDeactivateUserAsync(Guid userId)
    {
        var currentUserId = _claimService.GetUserId();

        // Admin không được deactivate chính mình
        if (currentUserId != null && Guid.Parse(currentUserId) == userId)
            throw new BadRequestException("You cannot deactivate your own account");

        var user = await _coreUserRepository.GetUserByIdAsync(userId);

        if (user == null)
            throw new NotFoundException($"User with id '{userId}' was not found");

        // Không được deactivate user có role System Administrator
        if (user.Role?.RoleName == "System Administrator")
            throw new BadRequestException("Cannot deactivate a System Administrator account");

        // Toggle trạng thái active
        user.IsActive = !user.IsActive;

        await _coreUserRepository.UpdateAsync(user);

        return new UserResponseModel
        {
            UserId      = user.UserId,
            Username    = user.Username,
            Email       = user.Email,
            Phonenumber = user.Phonenumber,
            RoleName    = user.Role?.RoleName,
            IsActive    = user.IsActive
        };
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var currentUserId = _claimService.GetUserId();

        // Admin không được tự xóa chính mình
        if (currentUserId != null && Guid.Parse(currentUserId) == userId)
            throw new BadRequestException("You cannot delete your own account");

        var user = await _coreUserRepository.GetUserByIdAsync(userId);

        if (user == null)
            throw new NotFoundException($"User with id '{userId}' was not found");

        // Không được xóa user có role System Administrator
        if (user.Role?.RoleName == "System Administrator")
            throw new BadRequestException("Cannot delete a System Administrator account");

        await _coreUserRepository.DeleteAsync(user);
    }

    public async Task<UserResponseModel> GetProfileAsync()
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        var user = await _coreUserRepository.GetUserByIdAsync(currentUserId);
        if (user == null)
            throw new NotFoundException("User not found");

        return new UserResponseModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Phonenumber = user.Phonenumber,
            RoleName = user.Role?.RoleName,
            IsActive = user.IsActive
        };
    }

    public async Task<UserResponseModel> UpdateProfileAsync(UpdateUserProfileModel model)
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        var user = await _coreUserRepository.GetUserByIdAsync(currentUserId);
        if (user == null)
            throw new NotFoundException("User not found");

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            if (string.IsNullOrEmpty(model.OldPassword))
                throw new BadRequestException("Old password is required when updating password");

            var isPasswordValid = PasswordHasher.VerifyPassword(model.OldPassword, user.Password);
            if (!isPasswordValid)
                throw new BadRequestException("Old password is incorrect");

            user.Password = PasswordHasher.HashPassword(model.NewPassword);
        }

        if (!string.IsNullOrWhiteSpace(model.Username) && model.Username != user.Username)
        {
            if (await _coreUserRepository.IsUsernameExistsExceptAsync(model.Username, currentUserId))
                throw new BadRequestException("Username is already taken by another user");
            user.Username = model.Username;
        }

        if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
        {
            if (await _coreUserRepository.IsEmailExistsExceptAsync(model.Email, currentUserId))
                throw new BadRequestException("Email is already in use by another user");
            user.Email = model.Email;
        }

        if (!string.IsNullOrWhiteSpace(model.Phonenumber) && model.Phonenumber != user.Phonenumber)
        {
            if (await _coreUserRepository.IsPhoneExistsExceptAsync(model.Phonenumber, currentUserId))
                throw new BadRequestException("Phone number is already in use by another user");
            user.Phonenumber = model.Phonenumber;
        }

        await _coreUserRepository.UpdateAsync(user);

        return new UserResponseModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Phonenumber = user.Phonenumber,
            RoleName = user.Role?.RoleName,
            IsActive = user.IsActive
        };
    }
}
