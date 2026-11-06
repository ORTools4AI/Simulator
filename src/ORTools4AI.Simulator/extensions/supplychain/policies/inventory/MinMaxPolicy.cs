// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.inventory;

// (s,S): order S-onHand when onHand < s
internal class MinMaxPolicy(double s, double S) : IInventoryPolicy
{
    public bool ShouldReview(SupplyChainNode node, Item item, TimeInstant now)
        => node.Inventory.OnHand(item) < s;

    public double DecideOrderQuantity(SupplyChainNode node, Item item, TimeInstant now)
        => Math.Max(0, S - node.Inventory.OnHand(item) - node.Inventory.OnReceipt(item));
}
