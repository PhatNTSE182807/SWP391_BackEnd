using System;

namespace N_Tier.Application.Models.User;

public class UserFollowingJournalResponseModel
{
    public Guid FollowId { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid JournalId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string JournalName { get; set; }
    
    public string NormalizedName { get; set; }
}
