// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

internal class Part : Entity
{
    internal string PartType { get; }
    internal TimeInstant CreatedAt { get; }
    private static int _counter;

    internal Part(Model owner, string partType, TimeInstant createdAt, bool showInTrace = false)
        : base(owner, $"Part-{partType}-{++_counter}", showInTrace)
    {
        PartType  = partType;
        CreatedAt = createdAt;
    }
}
