// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;

// Source from the upstream node with the highest on-hand inventory
internal class NearestSourcePolicy : ISourcingPolicy
{
    public IReadOnlyList<SupplyChainNode> GetCandidateSources(
        SupplyChainNode consumer, Item item, IReadOnlyList<SupplyChainNode> available)
        => [.. available.OrderByDescending(s => s.Inventory.OnHand(item))];

    public IReadOnlyDictionary<SupplyChainNode, double> AllocateOrder(
        SupplyChainNode consumer, Item item, double totalQuantity,
        IReadOnlyList<SupplyChainNode> sources, TimeInstant now)
    {
        var best = sources.MaxBy(s => s.Inventory.OnHand(item));
        if (best is null) return new Dictionary<SupplyChainNode, double>();
        return new Dictionary<SupplyChainNode, double> { [best] = totalQuantity };
    }
}
