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

    public List<Stroke> Order()
    {
        var groups = _strokes.GroupBy(x=>x.StroceColor).ToList();

        var anneal = new Annealing(_strokes);
        anneal.Anneal();

        return anneal.GetResult();
    }
}
