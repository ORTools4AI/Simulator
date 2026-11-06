// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.ai;

/// <summary>
/// Base class for AI-driven simulation processes.
/// Runs the observe → decide → act → reward loop at every
/// <see cref="DecisionInterval"/> ticks of simulated time.
///
/// Episode termination semantics (mirrors Gymnasium v0.26+):
///   terminated — override <see cref="IsTerminated"/> to return true when the
///                agent reaches a natural end state (e.g. budget exceeded,
///                stockout threshold hit). Do NOT bootstrap the value of
///                next_obs in the trajectory.
///   truncated  — set automatically when the experiment time limit is reached
///                before <see cref="IsTerminated"/> fires. next_obs is a valid
///                state and should be bootstrapped in offline RL.
///
/// The distinction is recorded in the <see cref="TrajectoryLogger{TObs,TAction}"/>
/// CSV (columns: terminated, truncated) and forwarded to
/// <see cref="IDecisionAgent{TObs,TAction}.OnEpisodeEnd"/>.
/// </summary>
public abstract class AiSimProcess<TObs, TAction> : SimProcess, IAiEpisodeListener
    where TObs : VectorObservation
{
    private readonly IDecisionAgent<TObs, TAction> _agent;
    private readonly IRewardFunction _reward;
    private readonly TrajectoryLogger<TObs, TAction>? _logger;

    private double _totalReward;
    private bool _exitedNaturally;

    // Buffers the most-recently completed step so the logger row can be
    // written with the correct nextObs (= obs at the start of the next step).
    private (TObs obs, TAction action, double r, bool terminated)? _pending;

    protected AiSimProcess(
        Model owner,
        IDecisionAgent<TObs, TAction> agent,
        IRewardFunction reward,
        TrajectoryLogger<TObs, TAction>? logger = null)
        : base(owner)
    {
        _agent  = agent;
        _reward = reward;
        _logger = logger;
        owner.RegisterAiProcess(this);
    }

    /// <summary>Simulated time between consecutive decisions. Default 1.0.</summary>
    protected virtual double DecisionInterval => 1.0;

    /// <summary>
    /// Extracts the current observation from simulation state.
    /// Must NOT advance simulation state — it may be called more than
    /// once per decision step when a logger is attached.
    /// </summary>
    protected abstract TObs BuildObservation();

    /// <summary>Applies the agent's chosen action to the simulation.</summary>
    protected abstract void ApplyAction(TAction action);

    /// <summary>
    /// Returns true when the agent has reached a natural terminal state.
    /// The episode ends with <c>terminated = true</c>, meaning no future
    /// value should be bootstrapped from this transition.
    /// Default: false (episodes always end by time limit → truncated).
    /// </summary>
    protected virtual bool IsTerminated() => false;

    protected override IEnumerable Run()
    {
        _totalReward     = 0;
        _exitedNaturally = false;
        _pending         = null;
        _reward.Reset();

        while (true)
        {
            yield return Delay(DecisionInterval);

            // obs is the state at the START of this step.
            // It is also nextObs for the PREVIOUS step → flush pending now.
            var obs = BuildObservation();

            if (_pending is var (po, pa, pr, pt))
                _logger?.Record(po, pa, pr, obs, terminated: pt, truncated: false);

            var action    = _agent.Decide(obs);
            ApplyAction(action);

            double r          = _reward.Compute();
            bool   terminated = IsTerminated();
            _totalReward += r;

            _pending = (obs, action, r, terminated);

            if (terminated)
            {
                _exitedNaturally = true;
                break;
            }
        }

        // Flush the terminal step. nextObs = current state; RL does not bootstrap it.
        if (_pending is var (fpo, fpa, fpr, _))
            _logger?.Record(fpo, fpa, fpr, BuildObservation(),
                            terminated: true, truncated: false);
        _pending = null;

        _agent.OnEpisodeEnd(_totalReward + _reward.ComputeTerminal(),
                            terminated: true, truncated: false);
    }

    // Delivered by Model.Run() after exp.finish() when the time limit fires
    // before IsTerminated() — the episode is truncated, not naturally done.
    void IAiEpisodeListener.NotifyTruncated()
    {
        if (_exitedNaturally) return;

        // _pending holds the last completed step. Its true nextObs is unknowable
        // (the process was suspended inside a Delay). Re-using obs as nextObs is
        // the standard approximation; TD(λ)/CQL/TD3+BC all bootstrap from it.
        if (_pending is var (po, pa, pr, _))
        {
            _logger?.Record(po, pa, pr, po, terminated: false, truncated: true);
            _pending = null;
        }

        _agent.OnEpisodeEnd(_totalReward + _reward.ComputeTerminal(),
                            terminated: false, truncated: true);
    }
}
