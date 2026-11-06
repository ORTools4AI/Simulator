// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.ddmrp;

// Tracks demand arrivals in a rolling window; computes Qualified Demand Spikes (QDS)
// for use in Net Flow Position = OnHand + OnOrder - QDS.
// Time values must be in a consistent unit decided by the caller (e.g. days or minutes).
internal class DdmrpDemandSpikeTracker(double spikeThreshold, double horizon)
{
    private readonly Queue<(double time, double qty)> _history = new();

    // Call from DemandProcess.OnDemandArrived; time in same unit as horizon.
    internal void RecordDemand(double currentTime, double qty)
        => _history.Enqueue((currentTime, qty));

    // Returns sum of spike-qualifying orders within the rolling horizon.
    internal double QualifiedDemandSpikes(double currentTime)
    {
        Purge(currentTime);
        double total = 0;
        foreach (var (_, qty) in _history)
            if (qty > spikeThreshold)
                total += qty;
        return total;
    }

    private void Purge(double currentTime)
    {
        double cutoff = currentTime - horizon;
        while (_history.Count > 0 && _history.Peek().time < cutoff)
            _history.Dequeue();
    }
}
