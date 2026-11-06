// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.extensions.supplychain.demand;

internal class PeriodicDemandPolicy : IDemandPolicy
{
    public NumericalDist<double> InterArrivalTimeDist { get; }
    public NumericalDist<double> OrderQuantityDist { get; }

    internal PeriodicDemandPolicy(Model owner, double interval,
        NumericalDist<double> quantityDist, string name = "PeriodicDemand")
    {
        InterArrivalTimeDist = new ContDistConstant(owner, name + "_Interval",
            interval, false, false);
        OrderQuantityDist = quantityDist;
    }
}
