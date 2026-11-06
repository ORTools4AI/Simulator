// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistBeta : ContDist
    {
        protected double alpha;
        protected double beta;

        public ContDistBeta(Model owner, string name, double alpha, double beta,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            this.alpha = alpha;
            this.beta = beta;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getAlpha() => alpha;
        public double getBeta() => beta;

        public override double sample()
        {
            double r1 = randomGenerator.nextDouble();
            double r2 = randomGenerator.nextDouble();
            incrementObservations();
            if (isAntithetic()) { r1 = 1 - r1; r2 = 1 - r2; }
            double g1 = MathNet.Numerics.Distributions.Gamma.InvCDF(alpha, 1.0, r1);
            double g2 = MathNet.Numerics.Distributions.Gamma.InvCDF(beta, 1.0, r2);
            double newSample = (g1 + g2) == 0 ? 0 : g1 / (g1 + g2);
            if (currentlySendTraceNotes()) traceLastSample(newSample.ToString());
            return newSample;
        }

        public override double getInverseOfCumulativeProbabilityFunction(double p)
            => throw new NotSupportedException(getName() + " does not support inverse cumulative probabilities.");
    }
}
