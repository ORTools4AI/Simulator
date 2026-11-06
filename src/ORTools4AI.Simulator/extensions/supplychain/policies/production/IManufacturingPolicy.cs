// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.production;

internal enum ManufacturingMode { MakeToStock, MakeToOrder, AssembleToOrder }

internal interface IManufacturingPolicy
{
    ManufacturingMode Mode { get; }
    double DecideProductionQuantity(Plant plant, Item finishedGood, TimeInstant now);
    IReadOnlyDictionary<Item, double> GetBomRequirements(Item finishedGood, double qty);
    NumericalDist<double> ProductionLeadTimeDist(Plant plant, Item finishedGood);
}
