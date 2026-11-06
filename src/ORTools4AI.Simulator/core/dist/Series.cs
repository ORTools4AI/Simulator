// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public abstract class Series<O> : Distribution
    {
        private readonly List<O> _allValList = [];
        private bool _direction;
        private bool _repeat;
        private int _index;

        public Series(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace)
        {
            _direction = false;
            _repeat = false;
            _index = 0;
        }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public void setReverse(bool direc)
        {
            if (!_direction && direc) _index -= 2;
            else if (_direction && !direc) _index += 2;
            _direction = direc;
        }

        public bool isforward() => !_direction;
        public void setRepeating(bool repeat) { _repeat = repeat; }
        public void remove(int position) { _allValList.RemoveAt(position); }
        public void clearList() { _allValList.Clear(); _index = 0; }
        public void add(O element) { _allValList.Add(element); }
        public void addAll(IEnumerable<O> collection) { _allValList.AddRange(collection); }
        public void set(O element, int position) { _allValList[position] = element; }

        public int getNumberOfAvailableElements()
        {
            if (_repeat) return int.MaxValue;
            return _direction ? _index + 1 : _allValList.Count - _index;
        }

        public virtual O? sample()
        {
            if (_allValList.Count == 0)
            {
                sendWarning("Failed to return sample value.", "Series : " + getName(), "List is empty.", "");
                return default;
            }
            if (!_direction)
            {
                if (_index < _allValList.Count)
                    return _allValList[_index++];
                if (_repeat) { _index = 1; return _allValList[0]; }
                sendWarning("List reached end.", "Series : " + getName(), "", "");
                return default;
            }
            else
            {
                if (_index >= 0)
                    return _allValList[_index--];
                if (_repeat) { _index = _allValList.Count - 2; return _allValList[^1]; }
                sendWarning("List reached end.", "Series : " + getName(), "", "");
                return default;
            }
        }

        public override object sampleObject() => sample()!;
    }
}
