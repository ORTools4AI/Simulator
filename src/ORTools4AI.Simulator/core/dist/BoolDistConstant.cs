// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class BoolDistConstant : BoolDist
    {
        protected bool constValue;

        public BoolDistConstant(Model owner, string name, bool constantValue,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            constValue = constantValue;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public bool getConstantValue() => constValue;

        public override bool sample()
        {
            incrementObservations();
            if (currentlySendTraceNotes()) traceLastSample(constValue.ToString());
            return constValue;
        }

        public void setConstant(bool newValue) { constValue = newValue; }
    }
}
