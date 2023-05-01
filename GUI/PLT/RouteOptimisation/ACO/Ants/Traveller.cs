using GUI.PLT.RouteOptimisation.ACO.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI.PLT.RouteOptimisation.ACO.Ants;

public class Traveller
{
    public Params Params { get; set; }
    public double FinalResult { get; set; }
    private Ant GlobalBestAnt { get; set; }
    public List<Edge> BestRoute { get; set; }
    private Graph Graph { get; set; }
    private string iterationsInfo;

    public Traveller(Params @params, Graph graph)
    {
        Params = @params;
        graph.MinimumPheromone = @params.InitialPheromone;
        Graph = graph;
        iterationsInfo = "";
        BestRoute = new List<Edge>();
    }

    public List<Edge> RunACS()
    {
        Graph.ResetPheromone(Params.InitialPheromone);
        for (int i = 0; i < Params.IterationAmount; i++)
        {
            iterationsInfo += $"Итерация {i + 1}" + "\n";
            List<Ant> antColony = CreateAnts();
            GlobalBestAnt = GlobalBestAnt ?? antColony[0];
            FinalResult = GlobalBestAnt.Distance;
            Ant localBestAnt = BuildTours(antColony);
            iterationsInfo += $"Длина лучшего пути: {localBestAnt.Distance} \n \n";
            if (Params.IterationAmount == 1)
            {
                GlobalBestAnt = localBestAnt;
                FinalResult = localBestAnt.Distance;
                BestRoute = localBestAnt.Path;
            }
            else
            {
                BestRoute = BestRoute ?? localBestAnt.Path;
                if (localBestAnt.Distance < GlobalBestAnt.Distance)
                {
                    GlobalBestAnt = localBestAnt;
                    FinalResult = localBestAnt.Distance;
                    BestRoute = localBestAnt.Path;
                }
            }
        }
        iterationsInfo += $"Финальный результат: {FinalResult}\n";
        //System.IO.File.WriteAllText("C:/Users/User/source/repos/AntColonyAlg/out.txt", iterationsInfo);

        return BestRoute;
    }

    public List<Ant> CreateAnts()
    {
        List<Ant> antColony = new List<Ant>();
        Random rand = new Random();
        for (int i = 0; i < Graph.Dimensions; i++)
        {
            int random = rand.Next(0, Graph.Dimensions);
            Ant ant = new Ant(Graph, Params.Alpha, Params.Beta);
            ant.Init(0);
            antColony.Add(ant);
        }
        return antColony;
    }

    public Ant BuildTours(List<Ant> antColony)
    {
        for (int i = 0; i < Graph.Dimensions; i++)
        {
            foreach (Ant ant in antColony)
            {
                ant.Step();
                AddPheromones(ant);
            }
        }

        foreach (var edge in Graph.Edges)
            Graph.SubstractPheromone(edge.Value, 1 - Params.EvaporationRate);

        for (int i = 0; i < antColony.Count; i++)
        {
            printPath(antColony[i], i);
        }

        var crunch = antColony.OrderBy(x => x.Distance).FirstOrDefault();
        printBestPath(crunch!);
        return crunch!; // find shortest ant tour (path)
    }

    public void AddPheromones(Ant ant)
    {
        double deltaR = 1 / ant.Distance;
        foreach (Edge edge in ant.Path)
        {
            double deposit = Params.EvaporationRate * deltaR;
            Graph.AddPheromone(edge, deposit);
        }
    }

    public void printBestPath(Ant ant)
    {
        int[] res = new int[ant.Path.Count + 1];
        string path = "Кратчайший путь: " + "\n";
        if (ant.Path[0].StartVertex == ant.Path[1].StartVertex || ant.Path[0].StartVertex == ant.Path[1].FinishVertex)
        {
            res[0] = ant.Path[0].FinishVertex;
            path += ant.Path[0].FinishVertex + " ";
        }
        else
        {
            res[0] = ant.Path[0].StartVertex;
            path += ant.Path[0].StartVertex + " ";
        }

        for (int i = 1; i < ant.Path.Count + 1; i++)
        {
            res[i] = -1;
        }

        for (int i = 0; i < ant.Path.Count; i++)
        {
            if (Array.IndexOf(res, ant.Path[i].StartVertex) <= -1)
            {
                res[i + 1] = ant.Path[i].StartVertex;
                path += ant.Path[i].StartVertex + " ";
            }
            else if (Array.IndexOf(res, ant.Path[i].FinishVertex) <= -1)
            {
                res[i + 1] = ant.Path[i].FinishVertex;
                path += ant.Path[i].FinishVertex + " ";
            }
        }
        iterationsInfo += path + "\n";
    }

    public void printPath(Ant ant, int n)
    {
        int[] res = new int[ant.Path.Count + 1];
        string path = $"Путь {n + 1}: " + "\n";
        if (ant.Path[0].StartVertex == ant.Path[1].StartVertex || ant.Path[0].StartVertex == ant.Path[1].FinishVertex)
        {
            res[0] = ant.Path[0].FinishVertex;
            path += ant.Path[0].FinishVertex + " ";
        }
        else
        {
            res[0] = ant.Path[0].StartVertex;
            path += ant.Path[0].StartVertex + " ";
        }

        for (int i = 1; i < ant.Path.Count + 1; i++)
        {
            res[i] = -1;
        }

        for (int i = 0; i < ant.Path.Count; i++)
        {
            if (Array.IndexOf(res, ant.Path[i].StartVertex) <= -1)
            {
                res[i + 1] = ant.Path[i].StartVertex;
                path += ant.Path[i].StartVertex + " ";
            }
            else if (Array.IndexOf(res, ant.Path[i].FinishVertex) <= -1)
            {
                res[i + 1] = ant.Path[i].FinishVertex;
                path += ant.Path[i].FinishVertex + " ";
            }
        }
        iterationsInfo += path + "\n";
    }
}
