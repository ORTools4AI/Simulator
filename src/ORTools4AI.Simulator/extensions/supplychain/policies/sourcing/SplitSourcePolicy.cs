// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;

// Distribute quantity across sources in fixed proportions (must sum to 1.0)
internal class SplitSourcePolicy(IReadOnlyDictionary<SupplyChainNode, double> proportions) : ISourcingPolicy
{
    public IReadOnlyList<SupplyChainNode> GetCandidateSources(
        SupplyChainNode consumer, Item item, IReadOnlyList<SupplyChainNode> available)
        => proportions.Keys.Where(s => available.Contains(s)).ToList();

    public IReadOnlyDictionary<SupplyChainNode, double> AllocateOrder(
        SupplyChainNode consumer, Item item, double totalQuantity,
        IReadOnlyList<SupplyChainNode> sources, TimeInstant now)
        => sources.ToDictionary(s => s,
            s => totalQuantity * proportions.GetValueOrDefault(s, 0.0));
}
