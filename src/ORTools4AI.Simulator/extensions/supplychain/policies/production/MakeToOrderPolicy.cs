// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.production;

internal class MakeToOrderPolicy : IManufacturingPolicy
{
    private readonly Dictionary<Item, Dictionary<Item, double>> _bom;
    private readonly NumericalDist<double> _leadTimeDist;

    public ManufacturingMode Mode => ManufacturingMode.MakeToOrder;

    internal MakeToOrderPolicy(Dictionary<Item, Dictionary<Item, double>> bom,
        NumericalDist<double> leadTimeDist)
    {
        _bom = bom;
        _leadTimeDist = leadTimeDist;
    }

    public double DecideProductionQuantity(Plant plant, Item finishedGood, TimeInstant now)
    {
        // produce the sum of all pending customer orders for this item
        double total = 0;
        foreach (var order in plant.PendingOrders)
            if (order.Item == finishedGood && order.Status == OrderStatus.Created)
                total += order.Quantity;
        return total;
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
