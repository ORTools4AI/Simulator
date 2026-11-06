// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Python;

/// <summary>
/// Concrete Model base class designed for Python subclassing via pythonnet.
///
/// ── Dispatch strategy ────────────────────────────────────────────────────
/// C#-internal virtual calls (from <c>connectToExperiment</c> and
/// <c>Experiment.start()</c>) do NOT dispatch to Python overrides.
/// This class accepts plain C# delegates (<see cref="Action"/>,
/// <see cref="Func{String}"/>) that Python registers at construction time.
/// When C# invokes those delegates, pythonnet calls the bound Python methods
/// directly — no virtual dispatch, no recursion.
///
/// The Python-level <c>ModelBase</c> wrapper sets all three callbacks
/// automatically; inherit from <c>ModelBase</c> in Python.
/// </summary>
public class PythonModel : Model
{
    private Action? _initCb;
    private Action? _doInitialSchedulesCb;
    private Func<string>? _descriptionCb;

    public PythonModel(Model? owner, string name, bool showInReport, bool showInTrace)
        : base(owner!, name, showInReport, showInTrace) { }

    public void SetInitCallback(Action cb)                  => _initCb = cb;
    public void SetDoInitialSchedulesCallback(Action cb)    => _doInitialSchedulesCb = cb;
    public void SetDescriptionCallback(Func<string> cb)     => _descriptionCb = cb;

    public override void   init()                   { _initCb?.Invoke(); }
    public override void   doInitialSchedules()     { _doInitialSchedulesCb?.Invoke(); }
    public override string description()            => _descriptionCb?.Invoke() ?? GetType().Name;
}
