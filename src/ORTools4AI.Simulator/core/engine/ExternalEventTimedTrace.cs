// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{
    public class ExternalEventTimedTrace : ExternalEvent
    {
        private string trueTime;
        private string what;

        public ExternalEventTimedTrace(string trueTime, int refUnit, string what, Model owner)
            : base(owner, "TimedTrace", true)
        {
            this.trueTime = trueTime;
            this.what = what;
        }

        public override void eventRoutine()
        {
            if (currentlySendTraceNotes())
                sendTraceNote(what + ": " + trueTime);
        }
    }
}
