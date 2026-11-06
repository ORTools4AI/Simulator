// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistTriangular : ContDist
    {
        protected double lower;
        protected double upper;
        protected double peak;

        public ContDistTriangular(Model owner, string name, double lower, double upper, double peak,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            this.lower = lower;
            this.upper = upper;
            this.peak = peak;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getLower() => lower;
        public double getUpper() => upper;
        public double getPeak() => peak;

        public override double sample() => base.sample();

        public override double getInverseOfCumulativeProbabilityFunction(double p)
        {
            double turningPoint = (peak - lower) / (upper - lower);
            if (p <= turningPoint)
                return Math.Sqrt(p * (upper - lower) * (peak - lower)) + lower;
            else
                return upper - Math.Sqrt((1 - p) * (upper - lower) * (upper - peak));
        }
    }
}
