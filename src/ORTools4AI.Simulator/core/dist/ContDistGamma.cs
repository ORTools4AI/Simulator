// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistGamma : ContDist
    {
        protected double alpha;
        protected double beta;

        public ContDistGamma(Model owner, string name, double alpha, double beta,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            this.alpha = alpha;
            this.beta = beta;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getAlpha() => alpha;
        public double getBeta() => beta;

        public override double sample() => base.sample();

        public override double getInverseOfCumulativeProbabilityFunction(double p)
            => MathNet.Numerics.Distributions.Gamma.InvCDF(alpha, 1.0 / beta, p);
    }
}
