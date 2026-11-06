// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.ai;

public interface IDecisionAgent<TObs, TAction>
{
    TAction Decide(TObs observation);

    /// <summary>
    /// Called once per episode when it ends.
    /// <paramref name="terminated"/> is true when the agent reached a natural
    /// terminal state inside the simulation (e.g. stockout limit hit, budget
    /// exhausted).  <paramref name="truncated"/> is true when the episode ended
    /// because the experiment time limit was reached, not because of any terminal
    /// state — the simulation could have continued.
    /// Exactly one of the two flags is true on each call.
    /// </summary>
    void OnEpisodeEnd(double totalReward, bool terminated, bool truncated) { }
}
