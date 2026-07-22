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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);

    /// <summary>
    /// Account status: true = active, false = deactivated
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete flag: true = deleted, false = normal
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Timestamp when the account was deleted (null if not deleted)
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public virtual Role Role { get; set; }

    public string FcmToken { get; set; }

    public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();

    public virtual ICollection<UserFollowingTopic> UserFollowingTopics { get; set; } = new List<UserFollowingTopic>();

    public virtual ICollection<UserFollowingJournal> UserFollowingJournals { get; set; } = new List<UserFollowingJournal>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

