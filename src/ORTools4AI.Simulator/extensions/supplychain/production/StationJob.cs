// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Immutable job token flowing through a station or batch gate.
// Use C# record `with` to produce a modified copy (e.g. wrapping OnComplete in ParallelStationGroup).
internal sealed record StationJob(
    IReadOnlyList<Part> Parts,
    string PartType,
    TimeInstant StartedAt,
    Action<IReadOnlyList<Part>>? OnComplete = null);
