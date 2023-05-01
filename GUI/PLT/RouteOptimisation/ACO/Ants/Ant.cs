using GUI.PLT.RouteOptimisation.ACO.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GUI.PLT.RouteOptimisation.ACO.Ants;

public class Ant
{
    public Graph Graph { get; set; }
    public int Alpha { get; set; }
    public int Beta { get; set; }
    public int StartNodeId { get; set; }
    public double Distance { get; set; }
    public List<int> VisitedNodes { get; set; }
    public List<int> UnvisitedNodes { get; set; }
    public List<Edge> Path { get; set; }

    public Ant(Graph graph, int alpha, int beta)
    {
        Graph = graph;
        Beta = beta;
        Alpha = alpha;
        VisitedNodes = new List<int>();
        UnvisitedNodes = new List<int>();
        Path = new List<Edge>();
    }

    public void Init(int startNodeId)
    {
        StartNodeId = startNodeId;
        Distance = 0;
        VisitedNodes.Add(startNodeId);
        for (int i = 0; i < Graph.Dimensions; i++)
        {
            if (i != startNodeId)
                UnvisitedNodes.Add(i);
        }
        Path.Clear();
    }

    public int CurrentNode() =>
        VisitedNodes[VisitedNodes.Count - 1];

    public Edge Step()
    {
        int endPoint;
        int startPoint = CurrentNode();
        startPoint = DrawStroke(startPoint);

        if (UnvisitedNodes.Count == 0)
        {
            endPoint = VisitedNodes[0];
        }
        else
        {
            Edge chosenEdge = NextEdge();
            if (chosenEdge.StartVertex != startPoint) endPoint = chosenEdge.StartVertex;
            else endPoint = chosenEdge.FinishVertex;
            VisitedNodes.Add(endPoint);
            UnvisitedNodes.RemoveAt(UnvisitedNodes.FindIndex(x => x == endPoint));
        }

        Edge edge = Graph.GetEdge(startPoint, endPoint);
        Path.Add(edge);
        Distance += edge.Length;
        return edge;
    }

    private Edge NextEdge()
    {
        List<Edge> edges = new();
        int currentNodeId = CurrentNode();

        foreach (var node in UnvisitedNodes)
        {
            var edge = Graph.GetEdge(currentNodeId, node);
            if (edge != null)
            {
                edge.Weight = Weight(edge.Length, edge.Pheromone);
                edges.Add(edge);
            }
        }

        return Search(edges);
    }

    private double Weight(double length, double pheromone) =>
        Math.Pow(pheromone, Alpha) * 1 / Math.Pow(length, Beta);

    private Edge Search(List<Edge> edges)
    {
        double totalSum = edges.Sum(x => x.Weight);
        var edgeP = edges.Select(w => { w.Weight = w.Weight / totalSum; return w; }).ToList();
        double sum = 0;
        foreach (var item in edgeP)
        {
            sum += item.Weight;
            item.Weight = sum;
        }

        double rand = new Random().NextDouble();

        return edgeP.First(j => j.Weight >= rand);
    }

    private int DrawStroke(int startId)
    {
        Edge edge;
        if (startId % 2 == 0)
        {
            if (VisitedNodes.Contains(startId + 1))
            {
                // всё плохо? пока хз
            }
            UnvisitedNodes.Remove(startId + 1);
            VisitedNodes.Add(startId + 1);
            edge = Graph.GetEdge(startId, startId + 1);
            Path.Add(edge);
            return startId + 1;
        }
        if (VisitedNodes.Contains(startId - 1))
        {
            // всё плохо? пока хз
        }
        UnvisitedNodes.Remove(startId - 1);
        VisitedNodes.Add(startId - 1);
        edge = Graph.GetEdge(startId, startId - 1);
        Path.Add(edge);
        return startId - 1;
    }
}
