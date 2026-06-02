using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.User;

namespace N_Tier.Application.Services;

public interface IUserService
{

    Task<List<UserResponseModel>> GetAllUsersAsync();



    Task<UserResponseModel> ToggleDeactivateUserAsync(Guid userId);
    Task DeleteUserAsync(Guid userId);

    Task<UserResponseModel> GetProfileAsync();

    Task<UserResponseModel> UpdateProfileAsync(UpdateUserProfileModel model);

    Task<List<UserBookmarkResponseModel>> GetBookmarksAsync();
    Task<UserBookmarkResponseModel> AddBookmarkAsync(Guid paperId);
    Task DeleteBookmarkAsync(Guid paperId);

    Task<List<UserFollowingTopicResponseModel>> GetFollowingTopicsAsync();
    Task<UserFollowingTopicResponseModel> FollowTopicAsync(Guid topicId);
    Task UnfollowTopicAsync(Guid topicId);
}
