using System.Collections.Generic;

namespace N_Tier.Application.Models.Analytics;

public class NetworkGraphDto
{
    public List<GraphNode> Nodes { get; set; } = new();
    public List<GraphEdge> Edges { get; set; } = new();
}

public class GraphNode
{
    public string Id { get; set; }
    public string Label { get; set; }
    public double Size { get; set; }
    public string Group { get; set; }
}

public class GraphEdge
{
    public string Source { get; set; }
    public string Target { get; set; }
    public double Weight { get; set; }
}
