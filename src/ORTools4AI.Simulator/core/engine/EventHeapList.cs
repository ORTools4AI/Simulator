// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine;

/// <summary>
/// EventList backed by an array-based binary min-heap (<see cref="BinaryHeapList{T}"/>).
/// Provides O(log n) insert/remove and O(1) peek, with better cache locality than the
/// red-black-tree-based <see cref="EventTreeList"/>.  All other EventList semantics
/// (insertAfter, insertBefore, traversal) are preserved — see BinaryHeapList for details.
/// </summary>
public class EventHeapList : EventTreeList
{
    public EventHeapList()
    {
        // Replace the PriorityQueue<EventNote> assigned by the base constructor.
        eTreeList = new BinaryHeapList<EventNote>();
    }
}
