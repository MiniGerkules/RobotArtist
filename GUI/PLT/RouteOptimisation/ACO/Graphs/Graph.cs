using GUI.PLT;
using System;
using System.Collections.Generic;

namespace GUI.PLT.RouteOptimisation.ACO.Graphs;

public class Graph
{
    public Dictionary<int, Edge> Edges { get; set; }
    public int Dimensions { get; set; }
    public double MinimumPheromone { get; set; }

    public Graph(int dimentions, List<Edge> edges)
    {
        Edges = new Dictionary<int, Edge>();
        Dimensions = dimentions;

        foreach (Edge edge in edges)
            if (!Edges.ContainsKey(HashFunction(edge.StartVertex, edge.FinishVertex)))
                Edges.Add(HashFunction(edge.StartVertex, edge.FinishVertex), edge);
    }
    public Edge GetEdge(int firstPointId, int secondPointId)
    {
        return Edges[HashFunction(firstPointId, secondPointId)];
    }
    public void ResetPheromone(double pheromoneValue)
    {
        foreach (var edge in Edges)
        {
            edge.Value.Pheromone = pheromoneValue;
        }
    }

    public void SubstractPheromone(Edge edge, double value)
    {
        edge.Pheromone = Math.Max(MinimumPheromone, edge.Pheromone * value);
    }

    public void AddPheromone(Edge edge, double value)
    {
        edge.Pheromone += value;
    }
    public int HashFunction(int x, int y)
    {
        return 10000000 * Math.Min(x, y) + Math.Max(x, y);
    }

    private void BuildGraph(List<Stroke> strikes)
    {

    }
}
