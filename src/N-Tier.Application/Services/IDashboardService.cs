using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Dashboard;

namespace N_Tier.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid userId);
    Task<IEnumerable<PublicationTrendDto>> GetPublicationTrendsAsync(int lastXMonths = 6);
    Task<IEnumerable<HotTopicDto>> GetHotTopicsAsync(int topCount = 5);
}
