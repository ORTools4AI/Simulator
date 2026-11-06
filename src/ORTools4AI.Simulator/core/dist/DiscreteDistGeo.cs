// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistGeo : DiscreteDist<long>
    {
        protected double probability;
        private double reverseProbPower;
        private readonly List<(long value, double cumProb)> _valueList = [];

        public DiscreteDistGeo(Model owner, string name, double probability,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            if (probability <= 0 || probability >= 1)
                sendWarning("Distribution not properly instantiated",
                    "DiscreteDistGeo : " + getName(), "Probability not in (0,1).", "");
            this.probability = probability;

            int storedAmount = (int)(Math.Log(0.01) / Math.Log(1 - probability));
            storedAmount = Math.Clamp(storedAmount, 5, 100);

            reverseProbPower = 1 - probability;
            for (int i = 0; i < storedAmount; i++)
            {
                _valueList.Add((i, 1 - reverseProbPower));
                reverseProbPower *= (1 - probability);
            }
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getProbability() => probability;

        public override long sample() => base.sample();

        public override long getInverseOfCumulativeProbabilityFunction(double p)
        {
            int i = 0;
            while (i < _valueList.Count && _valueList[i].cumProb < p) i++;
            if (i < _valueList.Count)
                return _valueList[i].value;
            double tmp = reverseProbPower;
            while ((1 - tmp) < p) { tmp *= (1 - probability); i++; }
            return i;
        }
    }
}
