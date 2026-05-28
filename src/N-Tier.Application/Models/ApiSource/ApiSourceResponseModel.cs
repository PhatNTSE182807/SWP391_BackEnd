using System;
using N_Tier.Application.Models;

namespace N_Tier.Application.Models.ApiSource;

public class ApiSourceResponseModel : BaseResponseModel
{
    public Guid SourceId { get; set; }
    public string SourceName { get; set; }
    public string BaseUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
