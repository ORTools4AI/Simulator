// Modified for ORTools4AI.Simulator.

using PartQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.extensions.supplychain.production.Part>;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Fixed-capacity FIFO buffer between simulation stages.
// TryAdd returns false when full; caller is responsible for backpressure logic.
internal sealed class FiniteBuffer
{
    private readonly int         _capacity;
    private readonly PartQueue _queue = new();

    internal int  Count   => _queue.Count;
    internal bool IsFull  => _queue.Count >= _capacity;
    internal bool IsEmpty => _queue.Count == 0;

    internal FiniteBuffer(int capacity) => _capacity = capacity;

    internal bool TryAdd(Part part)
    {
        if (IsFull) return false;
        _queue.Enqueue(part);
        return true;
    }

    internal Part? TryTake() => _queue.Count > 0 ? _queue.Dequeue() : null;
}
