namespace GUI.PLT.RouteOptimisation.ACO.Graphs;

public class Edge
{
    public int StartVertex { get; set; }
    public int FinishVertex { get; set; }
    public double Length { get; set; }
    public double Pheromone { get; set; }
    public double Weight { get; set; }

    public Edge() { }

    public Edge(int start, int end, double length)
    {
        StartVertex = start;
        FinishVertex = end;
        Length = length;
    }
}
