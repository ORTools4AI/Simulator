// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Python;

/// <summary>
/// Concrete SimProcess base class designed for Python subclassing via pythonnet.
///
/// ── Dispatch strategy ────────────────────────────────────────────────────
/// C#-internal virtual calls do NOT dispatch to Python overrides via pythonnet.
/// Instead, Python registers a <c>Func&lt;bool&gt;</c> delegate at construction
/// time.  Each invocation advances the Python generator by one step:
///   <c>True</c>  → generator yielded; <see cref="lifeCycle"/> suspends.
///   <c>False</c> → generator exhausted; <see cref="lifeCycle"/> returns.
///
/// The Python-level <c>SimProcessBase</c> wrapper sets the callback
/// automatically via <see cref="SetLifeCycleCallback"/>.
/// </summary>
public class PythonSimProcess : SimProcess
{
    private Func<bool>? _advanceCb;

    public PythonSimProcess(Model owner, string name, bool showInTrace)
        : base(owner, name, showInTrace) { }

    public PythonSimProcess(Model owner, string name, bool repeating, bool showInTrace)
        : base(owner, name, repeating, showInTrace) { }

    public void SetLifeCycleCallback(Func<bool> cb) => _advanceCb = cb;

    public sealed override IEnumerable<RunStatus> lifeCycle()
    {
        if (_advanceCb == null) yield break;
        while (_advanceCb())
            yield return RunStatus.passivate;
    }
}
