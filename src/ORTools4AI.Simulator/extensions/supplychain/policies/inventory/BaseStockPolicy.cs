// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.inventory;

// Order exactly what was consumed since last review to restore base-stock level B
internal class BaseStockPolicy(double B) : IInventoryPolicy
{
    private readonly Dictionary<(SupplyChainNode, Item), double> _lastOnHand = [];

    public bool ShouldReview(SupplyChainNode node, Item item, TimeInstant now) => true;

    public double DecideOrderQuantity(SupplyChainNode node, Item item, TimeInstant now)
    {
        var key = (node, item);
        double current = node.Inventory.OnHand(item);
        double last = _lastOnHand.GetValueOrDefault(key, B);
        double consumed = Math.Max(0, last - current);
        _lastOnHand[key] = current;
        // order up to B minus what's already on receipt
        double needed = B - current - node.Inventory.OnReceipt(item);
        return Math.Max(0, needed);
    }
}
