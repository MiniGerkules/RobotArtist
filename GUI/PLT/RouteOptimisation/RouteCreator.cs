using GUI.PLT.RouteOptimisation.ACO.Ants;
using GUI.PLT.RouteOptimisation.ACO.Graphs;
using GUI.PLT.RouteOptimisation.AnnealingAlg;
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
        var orderedGroups = new List<List<Stroke>>();
        var orderedList = new List<Stroke>();

        foreach (var group in groups)
        {
            var anneal = new Annealing(group.ToList());
            anneal.Anneal();
            orderedGroups.Add(anneal.GetResult());
        }
        foreach (var group in orderedGroups)
        {
            orderedList.Concat(group);
        }

        return orderedList;
    }

    public List <Stroke> ACOOrdering() 
    {
        var groups = _strokes.GroupBy(x => x.StroceColor).ToList();
        var orderedGroups = new List<List<Stroke>>();
        var orderedList = new List<Stroke>();
        foreach(var group in groups)
        {
            var strokes = group.ToList();
            var dimension = strokes.Count * 2 - 1;
            var @params = new Params();
            var read = new Reader(strokes);
            var graph = new Graph(dimension, read.edges);

            var traveller = new Traveller(@params, graph);
            traveller.RunACS();

            var res = traveller.BestRoute;
            var true_res = new List<Edge>();

            for (int i = 0; i < dimension - 1; i++)
            {
                true_res.Add(res[i]);
            }

            var converter = new StrokesCompiler(true_res, strokes);
            orderedGroups.Add(converter.ConvertToStrokes());
        }
        foreach(var group in orderedGroups)
        {
            orderedList.Concat(group);
        }

        return orderedList;
    }
}
