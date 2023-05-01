namespace GUI.PLT.RouteOptimisation.ACO.Ants;

public class Params
{
    public int Alpha { get; set; }
    public int Beta { get; set; }
    public double EvaporationRate { get; set; }
    public double InitialPheromone { get; set; }
    public int AntAmount { get; set; }
    public int IterationAmount { get; set; }

    public Params(int Alpha, int Beta, double EvaporationRate, double initialPheromone, int AntCount, int Iterations)
    {
        this.Alpha = Alpha;
        this.Beta = Beta;
        this.EvaporationRate = EvaporationRate;
        InitialPheromone = initialPheromone;
        AntAmount = AntCount;
        IterationAmount = Iterations;
    }

    public Params()
    {
        Alpha = 1;
        Beta = 2;
        EvaporationRate = 0.1;
        AntAmount = 20;
        IterationAmount = 100;
        InitialPheromone = 0.01;
    }
}
