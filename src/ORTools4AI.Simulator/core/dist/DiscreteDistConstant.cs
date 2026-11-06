// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistConstant<N> : DiscreteDist<N> where N : struct
    {
        private N _constValue;

        public DiscreteDistConstant(Model owner, string name, N constantValue,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            _constValue = constantValue;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public N getConstantValue() => _constValue;

        public override N sample() => base.sample();

        public override N getInverseOfCumulativeProbabilityFunction(double p) => _constValue;

        public void setConstant(N newValue)
        {
            double v = Convert.ToDouble(newValue);
            if (nonNegative && v < 0)
                sendWarning("You set a nonNegative DiscreteDistConstant to a negative constant.",
                    "DiscreteDistConstant: " + getName(), "", "");
            _constValue = newValue;
        }

        public new void setNonNegative(bool newValue)
        {
            if (newValue && Convert.ToDouble(_constValue) < 0)
                sendWarning("Attempt to set a DiscreteDistConstant with a negative constant to nonNegative.",
                    "DiscreteDistConstant: " + getName(), "", "");
            nonNegative = newValue;
        }
    }
}
