using System;
using N_Tier.Application.Models.User;

namespace N_Tier.Application.Models.UserBookmark
{
    public class UserBookmarkResponseModel
    {
        public Guid UserBookmarkId { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual UserResponseModel User { get; set; }
    }
}
