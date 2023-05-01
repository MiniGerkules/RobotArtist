using GeneralComponents;
using System;
using System.Collections.Generic;

namespace GUI.PLT.RouteOptimisation.ACO.Graphs;

public class Reader
{
    private List<Stroke> strokes;
    private List<GeneralComponents.Point2D> points;
    public List<Edge> edges;

    public Reader(List<Stroke> strokes)
    {
        points = new List<Point2D>();
        edges = new List<Edge>();
        this.strokes = strokes;
        foreach (Stroke stroke in strokes)
        {
            points.Add(stroke.Start);
            points.Add(stroke.End);
        }

        // верхнетреугольная матрица
        // кусочек полной матрицы смежности для графа
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                var d = CountDistance(points[i], points[j]);
                edges.Add(new Edge(i, j, d));
            }
        }
    }

    private double CountDistance(Point2D start, Point2D finish)
    {
        return Math.Sqrt(
            (start.X - finish.X) * (start.X - finish.X) +
            (start.Y - finish.Y) * (start.Y - finish.Y)
            );
    }
}
