// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistAggregate : ContDist
    {
        protected List<NumericalDist<double>> dists;
        protected Operator @operator;

        public ContDistAggregate(Model owner, string name, IList<NumericalDist<double>> distList,
            Operator op, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            @operator = op ?? Operator.PLUS;
            dists = distList.Where(d => d != null).ToList();
            if (dists.Count == 0)
            {
                dists.Add(new ContDistConstant(owner, "0", 0, false, false));
                sendWarning("No non-null distributions provided.", "ContDistAggregate: " + getName(), "", "");
            }
        }

        public ContDistAggregate(Model owner, string name, NumericalDist<double> dist1,
            NumericalDist<double> dist2, Operator op, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            @operator = op ?? Operator.PLUS;
            dists =
            [
                dist1 ?? new ContDistConstant(owner, "0", 0, false, false),
                dist2 ?? new ContDistConstant(owner, "0", 0, false, false)
            ];
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public IList<NumericalDist<double>> getDists() => new List<NumericalDist<double>>(dists);
        public Operator getOperator() => @operator;

        public override double sample()
        {
            incrementObservations();
            double newSample;
            do
            {
                newSample = dists[0].sample();
                for (int i = 1; i < dists.Count; i++)
                    newSample = @operator.result(newSample, dists[i].sample());
            } while (nonNegative && newSample < 0);
            if (currentlySendTraceNotes()) traceLastSample(newSample.ToString());
            return newSample;
        }

        public override double getInverseOfCumulativeProbabilityFunction(double p)
            => throw new NotSupportedException(getName() + " does not support inverse cumulative probabilities.");
    }
}
