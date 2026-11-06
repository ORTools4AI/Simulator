// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.ai;

/// <summary>
/// Records one row per decision step to a CSV file.
/// Columns: obs | action | reward | next_obs | terminated | truncated
///
/// terminated = true  : the agent reached a natural terminal state;
///                       do NOT bootstrap the value of next_obs.
/// truncated  = true  : the episode ended due to the experiment time limit;
///                       next_obs is a valid state — bootstrap normally.
/// Both flags are false on every non-final step.
/// Exactly one is true on the final step of each episode.
/// </summary>
public sealed class TrajectoryLogger<TObs, TAction> : IDisposable
    where TObs : VectorObservation
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public TrajectoryLogger(string path)
    {
        _writer = new StreamWriter(path, append: false);
        _writer.WriteLine("obs,action,reward,next_obs,terminated,truncated");
    }

    public void Record(TObs obs, TAction action, double reward,
                       TObs nextObs, bool terminated, bool truncated)
    {
        string o  = string.Join("|", obs.Features);
        string no = string.Join("|", nextObs.Features);
        _writer.WriteLine($"{o},{action},{reward:F4},{no},{terminated},{truncated}");
    }

    public void Flush() => _writer.Flush();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _writer.Dispose();
    }
}
