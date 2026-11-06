// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistPoisson : DiscreteDist<long>
    {
        protected double mean;

        public DiscreteDistPoisson(Model owner, string name, double meanValue,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            mean = meanValue;
            if (mean > 500)
                sendWarning("Mean > 500, samples will be inaccurate.",
                    "DiscreteDistPoisson : " + getName(), "Mean too large.", "");
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getMean() => mean;

        public override long sample()
        {
            incrementObservations();
            double l = Math.Exp(-mean);
            long k = 0;
            double p = 1;
            do
            {
                k++;
                p *= antithetic ? randomGenerator.nextDouble() : (1 - randomGenerator.nextDouble());
            } while (p > l);
            if (currentlySendTraceNotes()) traceLastSample((k - 1).ToString());
            return k - 1;
        }

        public override long getInverseOfCumulativeProbabilityFunction(double p)
        {
            if (p == 1.0) return long.MaxValue;
            double cumProb = 0;
            for (long x = 0; x < 10000; x++)
            {
                cumProb += MathNet.Numerics.Distributions.Poisson.PMF(mean, (int)x);
                if (cumProb >= p) return x;
            }
            return long.MaxValue;
        }
    }
}
