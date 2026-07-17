using System.Collections.Generic;
using N_Tier.Application.Models.Analytics;

namespace N_Tier.Application.Models.Dashboard;

public class HotTopicDto
{
    public string TopicName { get; set; }
    public int PaperCount { get; set; }
    public double GrowthPercentage { get; set; }
    public double TotalPercentage { get; set; }
    public List<YearlyCountDto> YearlyCounts { get; set; } = new();
}
