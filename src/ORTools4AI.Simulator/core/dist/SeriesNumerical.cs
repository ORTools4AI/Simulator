// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.core.dist
{
    public class SeriesNumerical<N> : Series<N>
    {
        protected double _sum;
        protected double _sumSquare;
        protected int _successfulObs;

        public SeriesNumerical(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        public override N? sample()
        {
            N? v = base.sample();
            if (v != null)
            {
                double d = Convert.ToDouble(v);
                _sum += d;
                _sumSquare += d * d;
                _successfulObs++;
            }
            return v;
        }

        public TimeSpan sampleTimeSpan()
        {
            N? s = sample();
            if (s == null) return TimeSpan.ZERO;
            double d = Convert.ToDouble(s);
            if (d < 0) { sendWarning("Negative sample, returning ZERO.", "SeriesNumerical " + getName(), "", ""); return TimeSpan.ZERO; }
            if (s is double || s is float) return new TimeSpan(d);
            return new TimeSpan(Convert.ToInt64(s));
        }

        public TimeSpan sampleTimeSpan(TimeUnit unit)
        {
            N? s = sample();
            if (s == null) return TimeSpan.ZERO;
            double d = Convert.ToDouble(s);
            if (d < 0) { sendWarning("Negative sample, returning ZERO.", "SeriesNumerical " + getName(), "", ""); return TimeSpan.ZERO; }
            if (s is double || s is float) return new TimeSpan(d, unit);
            return new TimeSpan(Convert.ToInt64(s), unit);
        }

        public double getMean()
        {
            if (_successfulObs == 0) { sendWarning("No samples yet.", "SeriesNumerical: " + getName(), "", ""); return -1.0; }
            return _sum / _successfulObs;
        }

        public double getStdDev()
        {
            if (_successfulObs < 2) { sendWarning("Fewer than 2 samples.", "SeriesNumerical: " + getName(), "", ""); return -1.0; }
            return Math.Sqrt(Math.Abs(_successfulObs * _sumSquare - _sum * _sum)
                / (_successfulObs * (_successfulObs - 1)));
        }
    }
}
