// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistCustom : ContDist
    {
        protected Function distFunction;
        protected double lowerBound;
        protected double upperBound;

        public ContDistCustom(Model owner, string name, Function function,
            double lower, double upper, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            distFunction = function;
            lowerBound = lower;
            upperBound = upper;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getLowerBound() => lowerBound;
        public double getUpperBound() => upperBound;
        public Function getFunction() => distFunction;

        public override double sample() => base.sample();

        public override double getInverseOfCumulativeProbabilityFunction(double p)
        {
            try
            {
                return MathNet.Numerics.RootFinding.Bisection.FindRoot(
                    x => distFunction.value(x) - p, lowerBound, upperBound, 1e-10, 100);
            }
            catch
            {
                sendWarning("Failed to find sample, returning NaN.",
                    "ContDistCustom : " + getName(), "Solver failed.", "Check function and bounds.");
                return double.NaN;
            }
        }
    }
}
