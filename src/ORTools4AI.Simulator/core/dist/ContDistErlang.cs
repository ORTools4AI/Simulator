// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistErlang : ContDistExponential
    {
        protected long k;

        public ContDistErlang(Model owner, string name, long order, double mean,
            bool showInReport, bool showInTrace)
            : base(owner, name, mean, showInReport, showInTrace)
        {
            k = order;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public new double getMean() => base.getMean();
        public long getOrder() => k;

        public override double sample()
        {
            double newSample = 0.0;
            for (int i = 1; i <= k; i++) newSample += base.sample();
            newSample /= k;
            incrementObservations(-(k - 1));
            if (currentlySendTraceNotes()) traceLastSample(newSample.ToString());
            return newSample;
        }

        public override double getInverseOfCumulativeProbabilityFunction(double p)
            => MathNet.Numerics.Distributions.Gamma.InvCDF(k, 1.0 / mean, p);
    }
}
