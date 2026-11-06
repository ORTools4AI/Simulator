// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.advancedModellingFeatures;

/// <summary>
/// Represents a scoped hold on units from a <see cref="Res"/> pool.
/// Created via <see cref="Res.Request"/>; acquired via
/// <see cref="ORTools4AI.Simulator.core.engine.SimProcess.Acquire"/>.
/// Dispose releases the units back to the pool.
/// </summary>
public sealed class ResLease : IDisposable
{
    internal readonly Res Res;
    internal readonly int N;
    internal bool Acquired;
    private bool _disposed;

    internal ResLease(Res res, int n) { Res = res; N = n; }

    public void Dispose()
    {
        if (!_disposed && Acquired)
        {
            _disposed = true;
            Res.takeBack(N);
        }
    }
}
