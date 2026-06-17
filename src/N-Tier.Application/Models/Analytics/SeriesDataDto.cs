using System.Collections.Generic;

namespace N_Tier.Application.Models.Analytics;

public class SeriesDataDto
{
    public string SeriesName { get; set; }
    public List<ChartDataPoint> DataPoints { get; set; } = new();
}
