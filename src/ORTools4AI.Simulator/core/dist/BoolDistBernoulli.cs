// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class BoolDistBernoulli : BoolDist
    {
        protected double trueProbability;

        public BoolDistBernoulli(Model owner, string name, double probabilityForTrue,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            trueProbability = probabilityForTrue;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getProbability() => trueProbability;

        public override bool sample()
        {
            incrementObservations();
            bool newSample = randomGenerator.nextDouble() < trueProbability;
            if (antithetic) newSample = !newSample;
            if (currentlySendTraceNotes()) traceLastSample(newSample.ToString());
            return newSample;
        }
    }
}
