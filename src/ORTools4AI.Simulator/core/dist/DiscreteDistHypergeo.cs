// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistHypergeo : DiscreteDist<long>
    {
        protected int setSize;
        protected int markedAmount;
        protected int subsetSize;
        private readonly List<(long value, double cumProb)> _valueList = [];

        public DiscreteDistHypergeo(Model owner, string name, int setSize,
            int markedAmount, int subsetSize, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            this.setSize = setSize;
            this.markedAmount = markedAmount;
            this.subsetSize = subsetSize;
            for (int i = 0; i <= subsetSize; i++)
                _valueList.Add((i, MathNet.Numerics.Distributions.Hypergeometric.CDF(setSize, markedAmount, subsetSize, i)));
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public int getSetSize() => setSize;
        public int getMarkedAmount() => markedAmount;
        public int getSubsetSize() => subsetSize;

        public override long sample() => base.sample();

        public override long getInverseOfCumulativeProbabilityFunction(double p)
        {
            int i = 0;
            while (i < _valueList.Count && _valueList[i].cumProb < p) i++;
            return _valueList[i].value;
        }
    }
}
