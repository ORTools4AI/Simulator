// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using DemandLog = System.Collections.Generic.Queue<(double time, double qty)>;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.ddmrp;

// Periodically recalculates ADU from recent demand history and updates buffer zones.
// Hook RecordDemand into DemandProcess.OnDemandArrived for the relevant node/item.
// All time values must use a consistent unit (adjustInterval and lookback in same unit
// as the simulation time, e.g. DAYS if the experiment runs in TimeUnit.DAYS).
internal class DdmrpBufferAdjuster : SimProcess
{
    private readonly DdmrpBuffer _buffer;
    private readonly double _adjustInterval;
    private readonly double _lookback;
    private readonly DemandLog _log = new();

    internal DdmrpBufferAdjuster(Model owner, SupplyChainNode node, Item item,
        DdmrpBuffer buffer, double adjustInterval, double lookback,
        bool showInTrace = false)
        : base(owner, $"DdmrpAdjuster-{node.NodeId}-{item.ItemId}", showInTrace)
    {
        _buffer = buffer;
        _adjustInterval = adjustInterval;
        _lookback = lookback;
    }

    // Register with DemandProcess.OnDemandArrived or call directly from a demand callback.
    // Pass time in the same unit as adjustInterval / lookback.
    internal void RecordDemand(double currentTime, double qty)
        => _log.Enqueue((currentTime, qty));

    protected override IEnumerable Run()
    {
        while (true)
        {
            yield return Delay(_adjustInterval);

            double now = presentTime().getTimeAsDouble(TimeUnit.DAYS);
            Purge(now);

            if (_log.Count > 0 && _lookback > 0)
                _buffer.Adu = _log.Sum(d => d.qty) / _lookback;
        }
    }

    private void Purge(double currentTime)
    {
        double cutoff = currentTime - _lookback;
        while (_log.Count > 0 && _log.Peek().time < cutoff)
            _log.Dequeue();
    }
}
