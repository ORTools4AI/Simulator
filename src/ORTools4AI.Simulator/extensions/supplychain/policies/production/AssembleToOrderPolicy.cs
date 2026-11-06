// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.production;

// Maintain component stock; assemble only on order receipt
internal class AssembleToOrderPolicy : IManufacturingPolicy
{
    private readonly Dictionary<Item, Dictionary<Item, double>> _bom;
    private readonly NumericalDist<double> _leadTimeDist;

    public ManufacturingMode Mode => ManufacturingMode.AssembleToOrder;

    internal AssembleToOrderPolicy(Dictionary<Item, Dictionary<Item, double>> bom,
        NumericalDist<double> leadTimeDist)
    {
        _bom = bom;
        _leadTimeDist = leadTimeDist;
    }

    public double DecideProductionQuantity(Plant plant, Item finishedGood, TimeInstant now)
    {
        double total = 0;
        foreach (var order in plant.PendingOrders)
            if (order.Item == finishedGood && order.Status == OrderStatus.Created)
                total += order.Quantity;
        // only assemble if components are available
        if (total <= 0) return 0;
        var bom = GetBomRequirements(finishedGood, total);
        foreach (var (component, required) in bom)
            if (plant.Inventory.OnHand(component) < required)
                return 0;
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
