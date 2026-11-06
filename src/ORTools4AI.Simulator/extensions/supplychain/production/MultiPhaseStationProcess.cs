// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;
using JobQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.extensions.supplychain.production.StationJob>;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Single-capacity station with N sequential processing phases and sequence-dependent setup.
// Setup fires only when the incoming part type differs from the last processed type.
internal class MultiPhaseStationProcess : SimProcess
{
    private readonly NumericalDist<double>[] _phases;
    private readonly Dictionary<(string from, string to), NumericalDist<double>> _setupMatrix;
    private string? _lastPartType;

    internal bool IsIdle => JobQueue.Count == 0 && !isScheduled();
    internal JobQueue JobQueue { get; } = new();

    internal MultiPhaseStationProcess(
        Model owner,
        string stationId,
        NumericalDist<double>[] phases,
        Dictionary<(string, string), NumericalDist<double>>? setupMatrix = null,
        bool showInTrace = false)
        : base(owner, $"Station-{stationId}", showInTrace)
    {
        _phases      = phases;
        _setupMatrix = setupMatrix ?? [];
    }

    internal void Enqueue(StationJob job)
    {
        JobQueue.Enqueue(job);
        if (!isScheduled())
            activate(new TimeSpan(0.0));
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            if (JobQueue.Count == 0)
            {
                // Enqueue() calls activate when a job arrives — wakeup is guaranteed externally.
                yield return Suspend();
                continue;
            }

            var job = JobQueue.Dequeue();

            if (_lastPartType is not null
                && _lastPartType != job.PartType
                && _setupMatrix.TryGetValue((_lastPartType, job.PartType), out var setupDist))
            {
                yield return Delay(setupDist.sample());
            }
            _lastPartType = job.PartType;

            foreach (var phase in _phases)
            {
                yield return Delay(phase.sample());
            }

            job.OnComplete?.Invoke(job.Parts);
        }
    }
}
