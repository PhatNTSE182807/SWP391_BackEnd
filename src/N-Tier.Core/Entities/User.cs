using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public Guid RoleId { get; set; }

    public string Phonenumber { get; set; }

    /// <summary>
    /// Trạng thái tài khoản: true = active, false = deactivated
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete: true = đã bị xóa, false = bình thường
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Thời điểm tài khoản bị xóa (null nếu chưa xóa)
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public virtual Role Role { get; set; }

    public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();

    public virtual ICollection<UserFollowingTopic> UserFollowingTopics { get; set; } = new List<UserFollowingTopic>();
}

