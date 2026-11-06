// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistNormal : ContDist
    {
        protected double mode;
        protected double stdDevLeft;
        protected double stdDevRight;
        protected double nextGaussian;
        protected bool haveNextGaussian;
        private bool symmetric;

        public ContDistNormal(Model owner, string name, double mean, double standardDeviation,
            bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            mode = mean;
            stdDevLeft = standardDeviation;
            stdDevRight = standardDeviation;
            symmetric = true;
        }

        public ContDistNormal(Model owner, string name, double mode, double stdDevLeft,
            double stdDevRight, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            this.mode = mode;
            this.stdDevLeft = stdDevLeft;
            this.stdDevRight = stdDevRight;
            symmetric = stdDevLeft == stdDevRight;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public double getMean() => isSymmetric()
            ? mode
            : mode + (stdDevRight - stdDevLeft) / Math.Sqrt(2 * Math.PI);

        public double getMode() => mode;
        public double getStdDev() => stdDevLeft;
        public double getStdDevLeft() => stdDevLeft;
        public double getStdDevRight() => stdDevRight;
        public bool isSymmetric() => symmetric;

        public override double sample()
        {
            double newSample;
            incrementObservations();
            do
            {
                if (haveNextGaussian)
                {
                    haveNextGaussian = false;
                    newSample = nextGaussian < 0
                        ? nextGaussian * stdDevLeft + mode
                        : nextGaussian * stdDevRight + mode;
                }
                else
                {
                    double v1, v2, s;
                    if (isAntithetic())
                    {
                        do
                        {
                            v1 = 2 * (1 - randomGenerator.nextDouble()) - 1;
                            v2 = 2 * (1 - randomGenerator.nextDouble()) - 1;
                            s = v1 * v1 + v2 * v2;
                        } while (s >= 1);
                    }
                    else
                    {
                        do
                        {
                            v1 = 2 * randomGenerator.nextDouble() - 1;
                            v2 = 2 * randomGenerator.nextDouble() - 1;
                            s = v1 * v1 + v2 * v2;
                        } while (s >= 1);
                    }
                    double mult = Math.Sqrt(-2 * Math.Log(s) / s);
                    nextGaussian = v2 * mult;
                    haveNextGaussian = true;
                    newSample = v1 < 0
                        ? v1 * mult * stdDevLeft + mode
                        : v1 * mult * stdDevRight + mode;
                }
            } while (nonNegative && newSample < 0);
            if (currentlySendTraceNotes()) traceLastSample(newSample.ToString());
            return newSample;
        }

        public override double getInverseOfCumulativeProbabilityFunction(double p)
            => MathNet.Numerics.Distributions.Normal.InvCDF(getMean(), getStdDev(), p);
    }
}
