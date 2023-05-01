using GUI.PLT;
using System;
using System.Collections.Generic;

namespace GUI.PLT.RouteOptimisation.ACO.Graphs;

public class StrokesCompiler
{
    private List<Edge> _edges;
    public List<Stroke> _strokes;

    public StrokesCompiler(List<Edge> edges, List<Stroke> strokes)
    {
        _edges = edges;
        _strokes = strokes;
    }

    public List<Stroke> ConvertToStrokes()
    {
        var result = new List<Stroke>();
        for (int i = 0; i < _edges.Count; i = i + 2)
        {
            var num = Math.Min(_edges[i].StartVertex, _edges[i].FinishVertex) / 2;
            if (_edges[i].StartVertex % 2 == 0)
            {
                result.Add(_strokes[num]);
                continue;
            }
            result.Add(new Stroke(
                _strokes[num].End,
                _strokes[num].Start,
                _strokes[num].StroceColor,
                _strokes[num].BrushWidth));
        }
        return result;
    }
}
