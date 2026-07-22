using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using N_Tier.Application.Common.Email;
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
    private readonly IDistributedCache _cache;
    private readonly IEmailService _emailService;

    public AuthService(
        ICoreUserRepository coreUserRepository,
        IConfiguration configuration,
        IDistributedCache cache,
        IEmailService emailService)
    {
        _coreUserRepository = coreUserRepository;
        _configuration = configuration;
        _cache = cache;
        _emailService = emailService;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
    {
        var user = await _coreUserRepository.GetUserWithRoleByEmailAsync(loginRequestModel.Email);

        if (user == null)
            throw new NotFoundException("Email or password is incorrect");

        if (!PasswordHasher.VerifyPassword(loginRequestModel.Password, user.Password))
            throw new BadRequestException("Email or password is incorrect");

        var token = user.IsActive ? JwtHelper.GenerateToken(user, user.Role.RoleName, _configuration) : null;

        return new LoginResponseModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Phonenumber = user.Phonenumber,
            RoleName = user.Role.RoleName,
            Token = token,
            IsVerified = user.IsActive
        };
    }

    public async Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel)
    {
        var roleName = GetRoleNameString(registerRequestModel.RoleName);

        // Check if the email belongs to a previously deleted account -> restore instead of creating a new one
        var deletedUser = await _coreUserRepository.GetDeletedUserByEmailAsync(registerRequestModel.Email);
        if (deletedUser != null)
        {
            var selectedRole = await _coreUserRepository.GetDefaultRoleAsync(roleName);
            if (selectedRole == null)
                throw new BadRequestException($"Role '{roleName}' does not exist");

            // Restore the old account with new information but set IsActive = false pending verification
            deletedUser.Username = registerRequestModel.Username;
            deletedUser.Phonenumber = registerRequestModel.PhoneNumber;
            deletedUser.Password = PasswordHasher.HashPassword(registerRequestModel.Password);
            deletedUser.RoleId = selectedRole.RoleId;
            deletedUser.IsDeleted = false;
            deletedUser.DeletedAt = null;
            deletedUser.IsActive = false;

            await _coreUserRepository.UpdateAsync(deletedUser);

            // Send OTP
            var otp = Random.Shared.Next(100000, 999999).ToString();
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync($"otp:register:{deletedUser.Email}", otp, cacheOptions);
            await SendVerificationEmailAsync(deletedUser.Email, otp);

            return new RegisterResponseModel
            {
                UserId = deletedUser.UserId,
                Username = deletedUser.Username,
                Email = deletedUser.Email,
                PhoneNumber = deletedUser.Phonenumber,
                RoleName = selectedRole.RoleName,
                IsVerified = false
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
            RoleId = selectedRole2.RoleId,
            IsActive = false // Account is not yet activated until OTP is verified
        };

        await _coreUserRepository.InsertAsync(newUser);

        // Send OTP
        var otp2 = Random.Shared.Next(100000, 999999).ToString();
        var cacheOptions2 = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync($"otp:register:{newUser.Email}", otp2, cacheOptions2);
        await SendVerificationEmailAsync(newUser.Email, otp2);

        return new RegisterResponseModel
        {
            UserId = newUser.UserId,
            Username = newUser.Username,
            Email = newUser.Email,
            PhoneNumber = newUser.Phonenumber,
            RoleName = selectedRole2.RoleName,
            IsVerified = false
        };
    }

    public async Task<bool> VerifyRegistrationAsync(VerifyRegistrationRequestModel model)
    {
        var cachedOtp = await _cache.GetStringAsync($"otp:register:{model.Email}");
        if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != model.Code)
            throw new BadRequestException("Invalid or expired verification code");

        var user = await _coreUserRepository.GetUserWithRoleByEmailAsync(model.Email);
        if (user == null)
            throw new NotFoundException($"User with email '{model.Email}' was not found");

        user.IsActive = true;
        await _coreUserRepository.UpdateAsync(user);

        await _cache.RemoveAsync($"otp:register:{model.Email}");

        return true;
    }

    public async Task ResendConfirmationCodeAsync(ResendConfirmationRequestModel model)
    {
        var user = await _coreUserRepository.GetUserWithRoleByEmailAsync(model.Email);
        if (user == null)
            throw new NotFoundException($"User with email '{model.Email}' was not found");

        if (user.IsActive)
            throw new BadRequestException("Account is already active");

        var otp = Random.Shared.Next(100000, 999999).ToString();
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync($"otp:register:{model.Email}", otp, cacheOptions);

        await SendVerificationEmailAsync(model.Email, otp);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestModel model)
    {
        var user = await _coreUserRepository.GetUserWithRoleByEmailAsync(model.Email);
        if (user == null)
            throw new NotFoundException($"User with email '{model.Email}' was not found");

        if (!user.IsActive)
            throw new BadRequestException("Account is deactivated");

        var token = Guid.NewGuid().ToString("N");
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
        await _cache.SetStringAsync($"reset-password:{token}", model.Email, cacheOptions);

        await SendForgotPasswordEmailAsync(model.Email, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestModel model)
    {
        var email = await _cache.GetStringAsync($"reset-password:{model.Token}");
        if (string.IsNullOrEmpty(email))
            throw new BadRequestException("Invalid or expired reset token");

        var user = await _coreUserRepository.GetUserWithRoleByEmailAsync(email);
        if (user == null)
            throw new NotFoundException($"User with email '{email}' was not found");

        user.Password = PasswordHasher.HashPassword(model.NewPassword);
        await _coreUserRepository.UpdateAsync(user);

        await _cache.RemoveAsync($"reset-password:{model.Token}");
    }

    private async Task SendVerificationEmailAsync(string email, string code)
    {
        var subject = "Confirm your Registration - Scientific Journal Tracking System";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                <h2 style='color: #007bff; text-align: center;'>Welcome to Scientific Journal System!</h2>
                <p>Thank you for registering. To activate your account, please use the following verification code:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #333333; background-color: #f5f5f5; padding: 10px 20px; border-radius: 4px; border: 1px dashed #cccccc;'>{code}</span>
                </div>
                <p style='color: #666666; font-size: 14px;'>This code is valid for 5 minutes. If you did not request this, please ignore this email.</p>
                <hr style='border: none; border-top: 1px solid #eeeeee; margin: 20px 0;'>
                <p style='font-size: 12px; color: #999999; text-align: center;'>Scientific Journal Tracking System &copy; {DateTime.UtcNow.Year}</p>
            </div>";

        var emailMessage = EmailMessage.Create(email, body, subject);
        await _emailService.SendEmailAsync(emailMessage);
    }

    private async Task SendForgotPasswordEmailAsync(string email, string token)
    {
        var subject = "Reset your Password - Scientific Journal Tracking System";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                <h2 style='color: #dc3545; text-align: center;'>Password Reset Request</h2>
                <p>You recently requested to reset the password for your Scientific Journal account.</p>
                <p>Please use the following token to complete the process:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <span style='font-size: 20px; font-weight: bold; color: #333333; background-color: #f8d7da; padding: 10px 20px; border-radius: 4px; border: 1px solid #f5c6cb; word-break: break-all; display: inline-block;'>{token}</span>
                </div>
                <p>Alternatively, if you are using our web interface, use this token in the Reset Password form.</p>
                <p style='color: #666666; font-size: 14px;'>This token is valid for 15 minutes. If you did not request a password reset, please ignore this email.</p>
                <hr style='border: none; border-top: 1px solid #eeeeee; margin: 20px 0;'>
                <p style='font-size: 12px; color: #999999; text-align: center;'>Scientific Journal Tracking System &copy; {DateTime.UtcNow.Year}</p>
            </div>";

        var emailMessage = EmailMessage.Create(email, body, subject);
        await _emailService.SendEmailAsync(emailMessage);
    }

    private static string GetRoleNameString(RoleNameEnum roleEnum) => roleEnum switch
    {
        RoleNameEnum.Researcher          => "Researcher",
        RoleNameEnum.Lecturer            => "Lecturer",
        RoleNameEnum.Student             => "Student",
        _                                => roleEnum.ToString()
    };
}
