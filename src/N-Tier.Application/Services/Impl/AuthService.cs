using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using N_Tier.Application.Exceptions;
using N_Tier.Application.Helpers;
using N_Tier.Application.Models.Auth;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Repositories;
using N_Tier.Shared.Helpers;

namespace N_Tier.Application.Services.Impl;

public class AuthService : IAuthService
{
    private readonly ICoreUserRepository _coreUserRepository;
    private readonly IConfiguration _configuration;

    public AuthService(ICoreUserRepository coreUserRepository, IConfiguration configuration)
    {
        _coreUserRepository = coreUserRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
    {
        var user = await _coreUserRepository.GetUserWithRoleByEmailAsync(loginRequestModel.Email);

        if (user == null)
            throw new NotFoundException("Email or password is incorrect");

        var isPasswordValid = PasswordHasher.VerifyPassword(loginRequestModel.Password, user.Password);

        if (!isPasswordValid)
            throw new BadRequestException("Email or password is incorrect");

        if (!user.IsActive)
            throw new BadRequestException("Account is currently deactivated!");

        var token = JwtHelper.GenerateToken(user, user.Role.RoleName, _configuration);

        return new LoginResponseModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Phonenumber = user.Phonenumber,
            RoleName = user.Role.RoleName,
            Token = token
        };
    }

    public async Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel)
    {
        var roleName = GetRoleNameString(registerRequestModel.RoleName);

        // Kiểm tra có phải email của tài khoản đã bị xóa không → restore thay vì tạo mới
        var deletedUser = await _coreUserRepository.GetDeletedUserByEmailAsync(registerRequestModel.Email);
        if (deletedUser != null)
        {
            var selectedRole = await _coreUserRepository.GetDefaultRoleAsync(roleName);
            if (selectedRole == null)
                throw new BadRequestException($"Role '{roleName}' does not exist");

            // Restore lại tài khoản cũ với thông tin mới
            deletedUser.Username = registerRequestModel.Username;
            deletedUser.Phonenumber = registerRequestModel.PhoneNumber;
            deletedUser.Password = PasswordHasher.HashPassword(registerRequestModel.Password);
            deletedUser.RoleId = selectedRole.RoleId;
            deletedUser.IsDeleted = false;
            deletedUser.DeletedAt = null;
            deletedUser.IsActive = true;

            await _coreUserRepository.UpdateAsync(deletedUser);

            return new RegisterResponseModel
            {
                UserId = deletedUser.UserId,
                Username = deletedUser.Username,
                Email = deletedUser.Email,
                PhoneNumber = deletedUser.Phonenumber,
                RoleName = selectedRole.RoleName
            };
        }

        if (await _coreUserRepository.IsUsernameExistsAsync(registerRequestModel.Username))
            throw new BadRequestException("User Name is already taken");

        if (await _coreUserRepository.IsEmailExistsAsync(registerRequestModel.Email))
            throw new BadRequestException("Email is already in use");

        if (await _coreUserRepository.IsPhoneExistsAsync(registerRequestModel.PhoneNumber))
            throw new BadRequestException("Phone number is already in use");

        var selectedRole2 = await _coreUserRepository.GetDefaultRoleAsync(roleName);

        if (selectedRole2 == null)
            throw new BadRequestException($"Role '{roleName}' does not exist");

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = registerRequestModel.Username,
            Email = registerRequestModel.Email,
            Phonenumber = registerRequestModel.PhoneNumber,
            Password = PasswordHasher.HashPassword(registerRequestModel.Password),
            RoleId = selectedRole2.RoleId
        };

        await _coreUserRepository.InsertAsync(newUser);

        return new RegisterResponseModel
        {
            UserId = newUser.UserId,
            Username = newUser.Username,
            Email = newUser.Email,
            PhoneNumber = newUser.Phonenumber,
            RoleName = selectedRole2.RoleName
        };
    }


    private static string GetRoleNameString(RoleNameEnum roleEnum) => roleEnum switch
    {
        RoleNameEnum.Researcher          => "Researcher",
        RoleNameEnum.Lecturer            => "Lecturer",
        RoleNameEnum.Student             => "Student",
        _                                => roleEnum.ToString()
    };
}
