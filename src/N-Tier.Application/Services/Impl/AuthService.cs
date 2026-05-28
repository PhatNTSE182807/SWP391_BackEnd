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
        var user = await _coreUserRepository.GetUserWithRoleByIdentifierAsync(loginRequestModel.Identifier);

        if (user == null)
            throw new NotFoundException("Identifier or password is incorrect");

        var isPasswordValid = PasswordHasher.VerifyPassword(loginRequestModel.Password, user.Password);

        if (!isPasswordValid)
            throw new BadRequestException("Identifier or password is incorrect");

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
        // Convert enum → tên role trong DB
        var roleName = GetRoleNameString(registerRequestModel.RoleName);

        // Kiểm tra trùng lặp username
        if (await _coreUserRepository.IsUsernameExistsAsync(registerRequestModel.Username))
            throw new BadRequestException("User Name is already taken");

        // Kiểm tra trùng lặp email
        if (await _coreUserRepository.IsEmailExistsAsync(registerRequestModel.Email))
            throw new BadRequestException("Email is already in use");

        // Kiểm tra trùng lặp phone
        if (await _coreUserRepository.IsPhoneExistsAsync(registerRequestModel.PhoneNumber))
            throw new BadRequestException("Phone number is already in use");

        // Kiểm tra role tồn tại trong DB
        var selectedRole = await _coreUserRepository.GetDefaultRoleAsync(roleName);

        if (selectedRole == null)
            throw new BadRequestException($"Role '{roleName}' does not exist");

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = registerRequestModel.Username,
            Email = registerRequestModel.Email,
            Phonenumber = registerRequestModel.PhoneNumber,
            Password = PasswordHasher.HashPassword(registerRequestModel.Password),
            RoleId = selectedRole.RoleId
        };

        await _coreUserRepository.InsertAsync(newUser);

        return new RegisterResponseModel
        {
            UserId = newUser.UserId,
            Username = newUser.Username,
            Email = newUser.Email,
            PhoneNumber = newUser.Phonenumber,
            RoleName = selectedRole.RoleName
        };
    }

    /// <summary>
    /// Chuyển đổi enum sang tên role chính xác trong database
    /// </summary>
    private static string GetRoleNameString(RoleNameEnum roleEnum) => roleEnum switch
    {
        RoleNameEnum.SystemAdministrator => "System Administrator",
        RoleNameEnum.Researcher          => "Researcher",
        RoleNameEnum.Lecturer            => "Lecturer",
        RoleNameEnum.Student             => "Student",
        _                                => roleEnum.ToString()
    };
}
