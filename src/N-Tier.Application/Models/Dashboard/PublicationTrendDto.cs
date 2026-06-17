using System.Collections.Generic;

namespace N_Tier.Application.Models.Dashboard;

public class PublicationTrendDto
{
    public string TopicName { get; set; }
    public List<MonthlyCountDto> MonthlyCounts { get; set; } = new();
}

public class MonthlyCountDto
{
    public string Month { get; set; } // e.g., "Jan", "Feb"
    public int Year { get; set; }
    public int MonthNumber { get; set; }
    public int Count { get; set; }
}
