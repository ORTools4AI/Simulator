// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistUniform : DiscreteDist<long>
    {
        private long _min;
        private long _max;

        public DiscreteDistUniform(Model owner, string name, long minValue, long maxValue,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            if (minValue <= maxValue) { _min = minValue; _max = maxValue; }
            else { _min = maxValue; _max = minValue; }
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public long getLower() => _min;
        public long getUpper() => _max;

        public override long sample() => base.sample();

        public override long getInverseOfCumulativeProbabilityFunction(double p)
        {
            long s = (long)Math.Floor(_min + ((_max - _min + 1) * p));
            if (s == _max + 1) s = _max;
            return s;
        }

        public new void setNonNegative(bool newValue)
        {
            if (newValue && _min < 0)
                sendWarning("Setting nonNegative on distribution with negative lowerBorder.",
                    "DiscreteDistUniform: " + getName(), "", "");
            if (newValue && _max < 0) { sendWarning("Negative upperBorder.", "DiscreteDistUniform: " + getName(), "", ""); return; }
            nonNegative = newValue;
        }
    }
}
