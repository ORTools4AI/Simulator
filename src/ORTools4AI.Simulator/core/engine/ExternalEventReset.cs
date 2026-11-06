// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{
    public class ExternalEventReset : ExternalEvent
    {
        public ExternalEventReset(Model owner, bool showInTrace)
            : base(owner, "Reset", showInTrace) { }

        public override void eventRoutine()
        {
            getModel().reset();
        }
    }
}
