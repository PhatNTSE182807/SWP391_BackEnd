using System;
using N_Tier.Application.Models.Paper;

namespace N_Tier.Application.Models.User;

public class UserBookmarkResponseModel
{
    public Guid BookmarkId { get; set; }
    public Guid UserId { get; set; }
    public Guid PaperId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public PaperResponseModel Paper { get; set; }
}
