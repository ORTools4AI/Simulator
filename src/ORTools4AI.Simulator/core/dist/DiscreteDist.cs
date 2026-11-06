// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{
    public abstract class DiscreteDist<N> : NumericalDist<N> where N : struct
    {
        public DiscreteDist(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }
    }
}
