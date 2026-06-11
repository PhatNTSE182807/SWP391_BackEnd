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
    private readonly IUserBookmarkRepository _userBookmarkRepository;
    private readonly IUserFollowingTopicRepository _userFollowingTopicRepository;
    private readonly IPaperRepository _paperRepository;
    private readonly IResearchTopicRepository _topicRepository;
    private readonly IClaimService _claimService;

    public UserService(
        ICoreUserRepository coreUserRepository, 
        IUserBookmarkRepository userBookmarkRepository,
        IUserFollowingTopicRepository userFollowingTopicRepository,
        IPaperRepository paperRepository,
        IResearchTopicRepository topicRepository,
        IClaimService claimService)
    {
        _coreUserRepository = coreUserRepository;
        _userBookmarkRepository = userBookmarkRepository;
        _userFollowingTopicRepository = userFollowingTopicRepository;
        _paperRepository = paperRepository;
        _topicRepository = topicRepository;
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

    public async Task<List<UserBookmarkResponseModel>> GetBookmarksAsync()
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        var bookmarks = await _userBookmarkRepository.GetBookmarksByUserIdAsync(currentUserId);
        
        return bookmarks.Select(b => new UserBookmarkResponseModel
        {
            BookmarkId = b.BookmarkId,
            UserId = b.UserId,
            PaperId = b.PaperId,
            CreatedAt = b.CreatedAt,
            Paper = b.Paper == null ? null : new Models.Paper.PaperResponseModel
            {
                Title = b.Paper.Title,
                Abstract = b.Paper.Abstract,
                Doi = b.Paper.Doi,
                PublicationYear = b.Paper.PublicationYear
                // Map other necessary properties here or use Mapster
            }
        }).ToList();
    }

    public async Task<UserBookmarkResponseModel> AddBookmarkAsync(Guid paperId)
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        // Validate paper
        var paper = await _paperRepository.GetFirstAsync(p => p.PaperId == paperId);
        if (paper == null)
            throw new NotFoundException($"Paper with id {paperId} not found");

        // Check if already bookmarked
        var isBookmarked = await _userBookmarkRepository.IsBookmarkedAsync(currentUserId, paperId);
        if (isBookmarked)
            throw new BadRequestException("You have already bookmarked this paper");

        var bookmark = new N_Tier.Core.Entities.UserBookmark
        {
            UserId = currentUserId,
            PaperId = paperId
        };

        var result = await _userBookmarkRepository.AddAsync(bookmark);

        return new UserBookmarkResponseModel
        {
            BookmarkId = result.BookmarkId,
            UserId = result.UserId,
            PaperId = result.PaperId,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task DeleteBookmarkAsync(Guid paperId)
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        var bookmark = await _userBookmarkRepository.GetBookmarkAsync(currentUserId, paperId);
        if (bookmark == null)
            throw new NotFoundException("Bookmark not found");

        await _userBookmarkRepository.DeleteAsync(bookmark);
    }

    public async Task<List<UserFollowingTopicResponseModel>> GetFollowingTopicsAsync()
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        var followingTopics = await _userFollowingTopicRepository.GetFollowingTopicsByUserIdAsync(currentUserId);

        return followingTopics.Select(f => new UserFollowingTopicResponseModel
        {
            FollowId = f.FollowId,
            UserId = f.UserId,
            TopicId = f.TopicId,
            CreatedAt = f.CreatedAt,
            TopicName = f.Topic?.TopicName,
            NormalizedName = f.Topic?.NormalizedName
        }).ToList();
    }

    public async Task<UserFollowingTopicResponseModel> FollowTopicAsync(Guid topicId)
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        // Validate topic
        var topic = await _topicRepository.GetFirstAsync(t => t.TopicId == topicId);
        if (topic == null)
            throw new NotFoundException($"Topic with id {topicId} not found");

        // Check if already following
        var isFollowing = await _userFollowingTopicRepository.IsFollowingAsync(currentUserId, topicId);
        if (isFollowing)
            throw new BadRequestException("You are already following this topic");

        var follow = new N_Tier.Core.Entities.UserFollowingTopic
        {
            UserId = currentUserId,
            TopicId = topicId
        };

        var result = await _userFollowingTopicRepository.AddAsync(follow);

        return new UserFollowingTopicResponseModel
        {
            FollowId = result.FollowId,
            UserId = result.UserId,
            TopicId = result.TopicId,
            CreatedAt = result.CreatedAt,
            TopicName = topic.TopicName,
            NormalizedName = topic.NormalizedName
        };
    }

    public async Task UnfollowTopicAsync(Guid topicId)
    {
        var currentUserIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
            throw new UnauthorizedException("User is not authenticated");

        var follow = await _userFollowingTopicRepository.GetFollowAsync(currentUserId, topicId);
        if (follow == null)
            throw new NotFoundException("Follow relationship not found");

        await _userFollowingTopicRepository.DeleteAsync(follow);
    }
}
