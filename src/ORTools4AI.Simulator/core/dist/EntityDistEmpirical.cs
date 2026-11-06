// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class EntityDistEmpirical<E> : EntityDist<E> where E : Entity
    {
        private readonly List<(E entity, double probability)> _entries = [];
        private bool _isInitialized;
        private double _totalProbabilities;

        public EntityDistEmpirical(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public bool add(E e, double probability)
        {
            if (probability < 0)
            {
                sendWarning("Can't add entry! Negative probability.", "EntityDistEmpirical " + getName(), "", "");
                return false;
            }
            if (contains(e))
            {
                sendWarning("Entity already in distribution.", "EntityDistEmpirical " + getName(), "", "");
                return false;
            }
            _entries.Add((e, probability));
            _totalProbabilities += probability;
            if (_totalProbabilities > 0.0) _isInitialized = true;
            return true;
        }

        public void changeProbability(E e, double newProbability)
        {
            if (newProbability < 0) { sendWarning("Negative probability.", "EntityDistEmpirical " + getName(), "", ""); return; }
            if (!contains(e)) { sendWarning("Entity not in distribution.", "EntityDistEmpirical " + getName(), "", ""); return; }
            for (int i = 0; i < _entries.Count; i++)
            {
                if (ReferenceEquals(_entries[i].entity, e))
                {
                    _totalProbabilities += newProbability - _entries[i].probability;
                    _entries[i] = (e, newProbability);
                    if (_totalProbabilities <= 0.0) _isInitialized = false;
                    return;
                }
            }
        }

        public bool contains(E e) => _entries.Any(x => ReferenceEquals(x.entity, e));

        public bool remove(E e)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (ReferenceEquals(_entries[i].entity, e))
                {
                    _totalProbabilities -= _entries[i].probability;
                    _entries.RemoveAt(i);
                    if (_totalProbabilities <= 0.0) _isInitialized = false;
                    return true;
                }
            }
            return false;
        }

        public override E? sample()
        {
            if (!_isInitialized)
            {
                sendWarning("Not initialized.", "EntityDistEmpirical : " + getName(), "", "");
                return null;
            }
            incrementObservations();
            double q = randomGenerator.nextDouble() * _totalProbabilities;
            if (antithetic) q = _totalProbabilities - q;
            double cum = 0;
            for (int i = 0; i < _entries.Count - 1; i++)
            {
                cum += _entries[i].probability;
                if (cum > q) return _entries[i].entity;
            }
            return _entries[^1].entity;
        }

        public new void setNonNegative(bool newValue)
        {
            nonNegative = newValue;
            sendWarning("setNonNegative on EntityDistEmpirical makes no sense.",
                "EntityDistEmpirical: " + getName(), "", "");
        }
    }
}
