// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistBinomial : DiscreteDist<long>
    {
        protected double probability;
        protected int amount;

        private readonly List<(long value, double cumProb)> _valueList = [];

        public DiscreteDistBinomial(Model owner, string name, double probability,
            int amount, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            this.probability = probability;
            this.amount = amount;
            for (int i = 0; i < amount; i++)
                _valueList.Add((i, MathNet.Numerics.Distributions.Binomial.CDF(probability, amount, i)));
            _valueList.Add((amount, 1.0));
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getProbability() => probability;
        public int getAmount() => amount;

        public override long sample() => base.sample();

        public override long getInverseOfCumulativeProbabilityFunction(double p)
        {
            int i = 0;
            while (i < _valueList.Count && _valueList[i].cumProb < p) i++;
            return _valueList[i].value;
        }
    }
}
