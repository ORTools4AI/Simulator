// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public abstract class EntityDist<E> : Distribution where E : Entity
    {
        public EntityDist(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        public abstract E? sample();

        public override object sampleObject() => sample()!;
    }
}
