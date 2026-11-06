// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.inventory;

internal interface IInventoryPolicy
{
    bool ShouldReview(SupplyChainNode node, Item item, TimeInstant now);
    double DecideOrderQuantity(SupplyChainNode node, Item item, TimeInstant now);
}
