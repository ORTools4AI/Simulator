// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistConstant : ContDist
    {
        private double _constValue;

        public ContDistConstant(Model owner, string name, double constantValue,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            _constValue = constantValue;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getConstantValue() => _constValue;

        public override double sample() => base.sample();

        public override double getInverseOfCumulativeProbabilityFunction(double p) => _constValue;

        public void setConstant(double newValue)
        {
            if (nonNegative && newValue < 0)
                sendWarning("Setting negative constant on nonNegative ContDistConstant.",
                    "ContDistConstant: " + getName(), "", "");
            _constValue = newValue;
        }

        public new void setNonNegative(bool newValue)
        {
            if (newValue && _constValue < 0)
                sendWarning("Negative constant on nonNegative ContDistConstant.",
                    "ContDistConstant: " + getName(), "", "");
            nonNegative = newValue;
        }
    }
}
