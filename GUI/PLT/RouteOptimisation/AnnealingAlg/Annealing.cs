using GeneralComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GUI.PLT.RouteOptimisation.AnnealingAlg;

public class Annealing
{
    private List<Stroke> strokes;
    private List<Stroke> currentOrder = new List<Stroke>();
    private List<Stroke> nextOrder = new List<Stroke>();
    private Random random = new Random();
    private double ShortestDistance;
    public double temperature = 10000.0;
    public double coolingRate = 0.9999;
    public double absoluteTemperature = 0.00001;

    const double eps = 1e-8;

    public Annealing(List<Stroke> strokes)
    {
        this.strokes = strokes;
        ShortestDistance = 0;
    }

    /// <summary>
    /// Calculate the total distance which is the objective function
    /// </summary>
    /// <param name="order"> A list containing coordinates of strokes </param>
    /// <returns></returns>
    private double GetTotalDistance(List<Stroke> order)
    {
        double distance = 0;

        for (int i = 0; i < order.Count - 1; i++)
        {
            distance += CountDistance(order[i].Start, order[i + 1].Start);
        }

        return distance;
    }

    /// <summary>
    /// Get the next random arrangements of strokes
    /// </summary>
    /// <param name="order"> A list containing coordinates of strokes </param>
    /// <returns></returns>
    private List<Stroke> GetNextArrangement(List<Stroke> order)
    {
        List<Stroke> newOrder = new List<Stroke>();

        for (int i = 0; i < order.Count; i++)
            newOrder.Add(order[i]);

        //we will only rearrange two strokes by random
        //starting point should be always zero - so zero should not be included

        int firstRandomCityIndex = random.Next(1, newOrder.Count);
        int secondRandomCityIndex = random.Next(1, newOrder.Count);

        var dummy = newOrder[firstRandomCityIndex];
        newOrder[firstRandomCityIndex] = newOrder[secondRandomCityIndex];
        newOrder[secondRandomCityIndex] = dummy;

        return newOrder;
    }

    /// <summary>
    /// Annealing Process
    /// </summary>
    public void Anneal()
    {
        int iteration = -1;
        double distance = GetTotalDistance(currentOrder);
        int crunch = 0;

        while (temperature > absoluteTemperature)
        {
            nextOrder = GetNextArrangement(currentOrder);
            var prev_d = 0.0;
            double deltaDistance = GetTotalDistance(nextOrder) - distance;

            if (Math.Abs(deltaDistance) < eps) crunch++;
            else crunch = 0;
            if (crunch > 50) break;

            //if the new order has a smaller distance
            //or if the new order has a larger distance
            //but satisfies Boltzman condition
            //then accept the arrangement
            if (deltaDistance < 0
                || distance > 0
                && Math.Exp(-deltaDistance / temperature) > random.NextDouble())
            {
                for (int i = 0; i < nextOrder.Count; i++)
                    currentOrder[i] = nextOrder[i];
                prev_d = distance;
                distance = deltaDistance + distance;

                if (Math.Abs(prev_d - distance) < eps) crunch++;
                else crunch = 0;
                if (crunch > 20) break;
            }

            //cool down the temperature
            temperature *= coolingRate;

            iteration++;
        }

        ShortestDistance = distance;
    }

    public double CountDistance(Point2D start, Point2D end)
    {
        return Math.Sqrt(
            (start.X - end.X) * (start.X - end.X)
            +
            (start.Y - end.Y) * (start.Y - end.Y));
    }

    public void Reset()
    {
        currentOrder = new List<Stroke>();
        nextOrder = new List<Stroke>();
        ShortestDistance = 0;
        temperature = 10000.0;
        coolingRate = 0.9999;
        absoluteTemperature = 0.00001;
    }

    public List<Stroke> GetResult()
    {
        return currentOrder;
    }
}