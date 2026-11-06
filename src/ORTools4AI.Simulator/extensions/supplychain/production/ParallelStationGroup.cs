// Modified for ORTools4AI.Simulator.

using WaitQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.extensions.supplychain.production.StationJob>;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Shared-queue dispatcher over N identical MultiPhaseStationProcess instances.
// Dispatches each job to the first idle station; queues the job if all are busy.
// When a station finishes, its wrapped OnComplete checks the wait queue and
// dispatches the next pending job to that same station.
internal sealed class ParallelStationGroup
{
    private readonly MultiPhaseStationProcess[] _stations;
    private readonly WaitQueue _waitQueue = new();

    internal int WaitQueueLength => _waitQueue.Count;

    internal ParallelStationGroup(MultiPhaseStationProcess[] stations)
    {
        _stations = stations;
    }

    internal void Enqueue(StationJob job)
    {
        var idle = Array.Find(_stations, s => s.IsIdle);
        if (idle is not null)
            idle.Enqueue(Wrap(job, idle));
        else
            _waitQueue.Enqueue(job);
    }

    private StationJob Wrap(StationJob job, MultiPhaseStationProcess station)
    {
        var original = job.OnComplete;
        return job with
        {
            OnComplete = parts =>
            {
                original?.Invoke(parts);
                if (_waitQueue.Count > 0)
                    station.Enqueue(Wrap(_waitQueue.Dequeue(), station));
            }
        };
    }
}
