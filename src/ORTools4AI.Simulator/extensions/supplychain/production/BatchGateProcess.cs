// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;
using PartQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.extensions.supplychain.production.Part>;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Accumulates arriving parts and releases a batch when either:
//   (a) pending count reaches minBatchSize  → immediate release via reActivate, or
//   (b) maxWaitMs elapses since first part  → release whatever has accumulated.
// Parts within one batch must share the same PartType; mixed batches are not supported.
internal class BatchGateProcess : SimProcess
{
    private readonly int    _minBatchSize;
    private readonly int    _maxBatchSize;
    private readonly double _maxWaitMs;
    private readonly Action<StationJob> _downstream;
    private readonly PartQueue _pending = new();

    internal BatchGateProcess(
        Model owner,
        string gateId,
        int    minBatchSize,
        double maxWaitMs,
        Action<StationJob> downstream,
        int    maxBatchSize = int.MaxValue,
        bool   showInTrace  = false)
        : base(owner, $"BatchGate-{gateId}", showInTrace)
    {
        _minBatchSize = minBatchSize;
        _maxBatchSize = maxBatchSize;
        _maxWaitMs    = maxWaitMs;
        _downstream   = downstream;
    }

    internal void Arrive(Part part)
    {
        _pending.Enqueue(part);
        if (!isScheduled())
            activate(new TimeSpan(0.0));           // wake from idle passivate
        else if (_pending.Count >= _minBatchSize)
            reActivate(new TimeSpan(0.0));          // cancel ongoing timeout hold — batch is ready
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            if (_pending.Count == 0)
            {
                // Arrive() calls activate when first part lands — wakeup guaranteed externally.
                yield return Suspend();
                continue;
            }

            if (_pending.Count < _minBatchSize)
            {
                yield return Delay(_maxWaitMs);
                // Woken by either: timeout expiry OR reActivate from Arrive() when batch fills.
            }

            ReleaseBatch();
        }
    }

    private void ReleaseBatch()
    {
        if (_pending.Count == 0) return;
        var batch = new List<Part>();
        while (_pending.Count > 0 && batch.Count < _maxBatchSize)
            batch.Add(_pending.Dequeue());
        _downstream(new StationJob(batch, batch[0].PartType, presentTime()));
    }
}
