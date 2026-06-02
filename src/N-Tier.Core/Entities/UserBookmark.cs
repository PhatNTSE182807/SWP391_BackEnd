using System;

namespace N_Tier.Core.Entities;

public class UserBookmark
{
    public Guid BookmarkId { get; set; }

    public Guid UserId { get; set; }

    public Guid PaperId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }

    public virtual Paper Paper { get; set; }
}
