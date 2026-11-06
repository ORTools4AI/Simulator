// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class DiscreteDistEmpirical<N> : DiscreteDist<N> where N : struct
    {
        private readonly List<(N value, double frequency)> _values = [];
        private bool _isInitialized;
        private double _totalProbabilities;

        public DiscreteDistEmpirical(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        public void addEntry(N value, double frequency)
        {
            if (frequency < 0)
            {
                sendWarning("Can't add empirical entry! Command ignored.",
                    "DiscreteDistEmpirical " + getName(), "Negative frequency.", "Use positive frequency.");
                return;
            }
            _values.Add((value, frequency));
            _isInitialized = true;
            _totalProbabilities += frequency;
        }

        public void removeEntry(N value)
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (_values[i].value.Equals(value))
                {
                    _totalProbabilities -= _values[i].frequency;
                    _values.RemoveAt(i);
                }
            }
            if (_values.Count == 0) _isInitialized = false;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public bool isInitialized() => _isInitialized;

        public override N sample() => base.sample();

        public override N getInverseOfCumulativeProbabilityFunction(double p)
        {
            if (!_isInitialized)
            {
                sendWarning("Invalid sample returned!", "DiscreteDistEmpirical: " + getName(),
                    "Not initialized.", "Add entries first.");
                return default;
            }
            double cum = 0;
            int i = 0;
            while (true)
            {
                cum += _values[i].frequency / _totalProbabilities;
                if (cum >= p) break;
                i++;
            }
            return _values[i].value;
        }
    }
}
