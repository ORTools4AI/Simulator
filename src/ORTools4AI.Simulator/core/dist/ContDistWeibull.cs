// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistWeibull : ContDistExponential
    {
        protected double beta;

        public ContDistWeibull(Model owner, string name, double mean, double beta,
            bool showInReport, bool showInTrace)
            : base(owner, name, mean, showInReport, showInTrace)
        {
            this.beta = beta;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getBeta() => beta;

        public override double sample() => base.sample();

        public override double getInverseOfCumulativeProbabilityFunction(double p)
            => Math.Pow(base.getInverseOfCumulativeProbabilityFunction(p), 1.0 / beta);
    }
}
