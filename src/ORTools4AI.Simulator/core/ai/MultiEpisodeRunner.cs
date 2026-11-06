// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.ai;

public record EpisodeResult(int Episode, double TotalReward, long Seed);

/// <summary>
/// Runs a simulation model for multiple independent episodes and aggregates results.
/// </summary>
public sealed class MultiEpisodeRunner<TModel>(
    Func<TModel> modelFactory,
    double durationMinutes,
    int episodes = 10)
    where TModel : Model
{
    public EpisodeResult[] Run(Func<TModel, IDecisionAgent<VectorObservation, int>> agentFactory)
    {
        var results = new EpisodeResult[episodes];
        var rng     = new Random(42);

        for (int i = 0; i < episodes; i++)
        {
            long seed  = rng.NextInt64();
            var  model = modelFactory();
            agentFactory(model); // agent registers itself on model during construction
            model.Run(durationMinutes, TimeUnit.MINUTES, seed);
            results[i] = new EpisodeResult(i, 0, seed);
        }

        return results;
    }

    /// <summary>
    /// Runs two agents over the same episodes (same seeds) and prints a comparison table.
    /// </summary>
    public void Compare(
        string nameA, Func<TModel, IDecisionAgent<VectorObservation, int>> agentA,
        string nameB, Func<TModel, IDecisionAgent<VectorObservation, int>> agentB)
    {
        var resultsA = Run(agentA);
        var resultsB = Run(agentB);

        Console.WriteLine($"\n{"Agent",-24} {"Mean Reward",14} {"Std Dev",10} {"Episodes",10}");
        Console.WriteLine(new string('-', 60));
        PrintRow(nameA, resultsA);
        PrintRow(nameB, resultsB);
        Console.WriteLine();
    }

    private static void PrintRow(string name, EpisodeResult[] r)
    {
        double mean  = r.Average(e => e.TotalReward);
        double stdev = Math.Sqrt(r.Average(e => Math.Pow(e.TotalReward - mean, 2)));
        Console.WriteLine($"{name,-24} {mean,14:F2} {stdev,10:F2} {r.Length,10}");
    }
}
