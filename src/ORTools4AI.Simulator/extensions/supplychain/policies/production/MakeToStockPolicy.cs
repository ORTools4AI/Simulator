// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.production;

internal class MakeToStockPolicy : IManufacturingPolicy
{
    private readonly double _reorderPoint;
    private readonly double _targetLevel;
    private readonly Dictionary<Item, Dictionary<Item, double>> _bom;
    private readonly NumericalDist<double> _leadTimeDist;

    public ManufacturingMode Mode => ManufacturingMode.MakeToStock;

    internal MakeToStockPolicy(double reorderPoint, double targetLevel,
        Dictionary<Item, Dictionary<Item, double>> bom, NumericalDist<double> leadTimeDist)
    {
        _reorderPoint = reorderPoint;
        _targetLevel = targetLevel;
        _bom = bom;
        _leadTimeDist = leadTimeDist;
    }

    public double DecideProductionQuantity(Plant plant, Item finishedGood, TimeInstant now)
    {
        double onHand = plant.Inventory.OnHand(finishedGood);
        return onHand < _reorderPoint ? Math.Max(0, _targetLevel - onHand) : 0;
    }

    public IReadOnlyDictionary<Item, double> GetBomRequirements(Item finishedGood, double qty)
    {
        if (!_bom.TryGetValue(finishedGood, out var requirements))
            return new Dictionary<Item, double>();
        return requirements.ToDictionary(kv => kv.Key, kv => kv.Value * qty);
    }

    public NumericalDist<double> ProductionLeadTimeDist(Plant plant, Item finishedGood)
        => _leadTimeDist;
}
