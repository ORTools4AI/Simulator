// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.extensions.supplychain.demand;

internal class PoissonDemandPolicy : IDemandPolicy
{
    public NumericalDist<double> InterArrivalTimeDist { get; }
    public NumericalDist<double> OrderQuantityDist { get; }

    internal PoissonDemandPolicy(Model owner, double arrivalRate, double meanQty,
        string name = "PoissonDemand")
    {
        // inter-arrival time is 1/arrivalRate for a Poisson process
        var interArrival = new ContDistExponential(owner, name + "_InterArrival",
            1.0 / arrivalRate, false, false);
        interArrival.setNonNegative(true);
        InterArrivalTimeDist = interArrival;

        var qty = new ContDistExponential(owner, name + "_Qty", meanQty, false, false);
        qty.setNonNegative(true);
        OrderQuantityDist = qty;
    }
}
