// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.ai;

/// <summary>
/// Implemented by <see cref="AiSimProcess{TObs,TAction}"/> so that
/// <see cref="ORTools4AI.Simulator.core.engine.Model.Run"/> can notify all
/// AI processes at episode end, allowing them to flush their pending
/// trajectory row with the correct <c>truncated = true</c> flag.
/// </summary>
internal interface IAiEpisodeListener
{
    void NotifyTruncated();
}
