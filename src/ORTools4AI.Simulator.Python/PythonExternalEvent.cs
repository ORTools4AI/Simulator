// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Python;

/// <summary>
/// Concrete zero-entity ExternalEvent base class designed for Python
/// subclassing via pythonnet.
///
/// ── Dispatch strategy ────────────────────────────────────────────────────
/// Python registers a plain <see cref="Action"/> delegate at construction
/// time.  When the event fires, <see cref="eventRoutine"/> invokes that
/// delegate, which calls the Python method directly — no C# virtual dispatch,
/// no recursion.
///
/// The Python-level <c>ExternalEventBase</c> wrapper sets the callback
/// automatically via <see cref="SetEventCallback"/>.
/// </summary>
public class PythonExternalEvent : ExternalEvent
{
    private Action? _eventCb;

    public PythonExternalEvent(Model owner, string name, bool showInTrace)
        : base(owner, name, showInTrace) { }

    public void SetEventCallback(Action cb) => _eventCb = cb;

    public override void eventRoutine() => _eventCb?.Invoke();
}
