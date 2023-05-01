using Algorithm;
using GUI.PLT.RouteOptimisation.ACO.Ants;
using GUI.PLT.RouteOptimisation.ACO.Graphs;
using GUI.PLT.RouteOptimisation.AnnealingAlg;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI.PLT.RouteOptimisation;

internal class RouteCreator
{
    private List<Stroke> _strokes;

    RouteCreator(List<Stroke> strokes)
    {
        _strokes = strokes;
    }

    public List<Stroke> AnnealingOrdering()
    {
        var groups = _strokes.GroupBy(x=>x.StroceColor).ToList();

        var anneal = new Annealing(_strokes);
        anneal.Anneal();

        return anneal.GetResult();
    }

    public List <Stroke> ACOOrdering() 
    {
        var dimension = _strokes.Count * 2 - 1;

        var @params = new Params();

        var read = new Reader(_strokes);

        var graph = new Graph(dimension, read.edges);

        var traveller = new Traveller(@params, graph);
        traveller.RunACS();

        var res = traveller.BestRoute;
        var true_res = new List<Edge>();

        for (int i = 0; i < dimension - 1; i++)
        {
            true_res.Add(res[i]);
        }

        // каждое нечётное ребро пути - мазок
        // преобразуем обратно рёбра в мазки

        var converter = new StrokesCompiler(true_res, _strokes);
        return converter.ConvertToStrokes();
    }
}
