// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.inventory;

// (r,Q): order fixed Q when onHand <= r
internal class FixedOrderQuantityPolicy(double r, double Q) : IInventoryPolicy
{
    public bool ShouldReview(SupplyChainNode node, Item item, TimeInstant now)
        => node.Inventory.OnHand(item) <= r;

    public double DecideOrderQuantity(SupplyChainNode node, Item item, TimeInstant now)
        => Q;
}
