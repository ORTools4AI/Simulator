// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class ContDistEmpirical : ContDist
    {
        private readonly List<(double value, double frequency)> _values = [];
        private bool _isInitialized;
        private bool _cumProZeroInitialized;
        private bool _cumProOneInitialized;

        public ContDistEmpirical(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public bool isInitialized() => _isInitialized;

        public void addEntry(double value, double frequency)
        {
            if (frequency < 0.0 || frequency > 1.0)
            {
                sendWarning("Can't add empirical entry! Command ignored.",
                    "ContDistEmpirical : " + getName(), "Frequency out of [0,1].", "");
                return;
            }
            for (int i = 0; i < _values.Count; i++)
            {
                if (_values[i].value == value) { sendWarning("Duplicate value.", "ContDistEmpirical : " + getName(), "", ""); return; }
                if (_values[i].value > value)
                {
                    if (i == 0 && _values[i].frequency >= frequency) { _addAt(i, value, frequency); return; }
                    if (i == 0 || _values[i].frequency < frequency || _values[i - 1].frequency > frequency)
                    { sendWarning("Entry does not fit list.", "ContDistEmpirical : " + getName(), "", ""); return; }
                    _addAt(i, value, frequency);
                    return;
                }
            }
            if (_values.Count == 0) _addAt(0, value, frequency);
            else if (_values[^1].frequency <= frequency) _addAt(_values.Count, value, frequency);
        }

        private void _addAt(int pos, double value, double frequency)
        {
            if (frequency == 0) { _cumProZeroInitialized = true; if (_cumProOneInitialized) _isInitialized = true; }
            if (frequency == 1) { _cumProOneInitialized = true; if (_cumProZeroInitialized) _isInitialized = true; }
            _values.Insert(pos, (value, frequency));
        }

        public override double sample() => base.sample();

        public override double getInverseOfCumulativeProbabilityFunction(double p)
        {
            if (!_isInitialized)
            {
                sendWarning("Invalid sample returned!", "ContDistEmpirical : " + getName(), "Not initialized.", "");
                return 0.0;
            }
            int i = 1;
            while (_values[i].frequency < p) i++;
            double lowVal = _values[i - 1].value, lowFreq = _values[i - 1].frequency;
            double highVal = _values[i].value, highFreq = _values[i].frequency;
            return lowVal + ((highVal - lowVal) * (p - lowFreq)) / (highFreq - lowFreq);
        }
    }
}
