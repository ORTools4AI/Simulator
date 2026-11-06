// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Python;

/// <summary>
/// Concrete single-entity Event base class designed for Python subclassing
/// via pythonnet.
///
/// ── Dispatch strategy ────────────────────────────────────────────────────
/// Python registers an <see cref="Action{Entity}"/> delegate at construction
/// time.  When the event fires, <see cref="eventRoutine"/> invokes that
/// delegate, passing the entity.  No C# virtual dispatch, no recursion.
///
/// The Python-level <c>EventBase</c> wrapper sets the callback automatically
/// via <see cref="SetEventCallback"/>.
/// </summary>
public class PythonEvent : Event
{
    private Action<Entity>? _eventCb;

    public PythonEvent(Model owner, string name, bool showInTrace)
        : base(owner, name, showInTrace) { }

    public void SetEventCallback(Action<Entity> cb) => _eventCb = cb;

    public override void eventRoutine(Entity who) => _eventCb?.Invoke(who);
}
