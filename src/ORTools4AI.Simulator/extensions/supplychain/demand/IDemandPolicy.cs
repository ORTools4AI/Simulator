// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;

namespace ORTools4AI.Simulator.extensions.supplychain.demand;

internal interface IDemandPolicy
{
    NumericalDist<double> InterArrivalTimeDist { get; }
    NumericalDist<double> OrderQuantityDist { get; }
}
