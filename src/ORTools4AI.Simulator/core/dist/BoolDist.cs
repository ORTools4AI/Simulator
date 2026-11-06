// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public abstract class BoolDist : Distribution
    {
        public BoolDist(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        public abstract bool sample();

        public override object sampleObject() => sample();
    }
}
