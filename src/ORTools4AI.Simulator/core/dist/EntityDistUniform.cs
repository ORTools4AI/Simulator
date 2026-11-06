// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public class EntityDistUniform<E> : EntityDist<E> where E : Entity
    {
        private readonly List<E> _entities = [];
        private bool _isInitialized;

        public EntityDistUniform(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        protected override Reporter createDefaultReporter() => new DistributionReporter(this);

        public bool add(E e)
        {
            _isInitialized = true;
            if (contains(e)) { sendWarning("Entity already present.", "EntityDistUniform " + getName(), "", ""); return false; }
            _entities.Add(e);
            return true;
        }

        public bool contains(E e) => _entities.Contains(e);

        public bool remove(E e)
        {
            bool result = _entities.Remove(e);
            if (_entities.Count == 0) _isInitialized = false;
            return result;
        }

        public override E? sample()
        {
            if (!_isInitialized) { sendWarning("Not initialized.", "EntityDistUniform : " + getName(), "", ""); return null; }
            incrementObservations();
            int max = _entities.Count - 1;
            int index;
            do
            {
                double r = isAntithetic() ? (1 - randomGenerator.nextDouble()) : randomGenerator.nextDouble();
                index = (int)(Math.Floor((double)(max + 1)) * r);
            } while (index == max + 1);
            return _entities[index];
        }

        public new void setNonNegative(bool newValue)
        {
            nonNegative = newValue;
            sendWarning("setNonNegative on EntityDistUniform makes no sense.",
                "EntityDistUniform: " + getName(), "", "");
        }
    }
}
