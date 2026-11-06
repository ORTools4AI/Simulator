// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;

internal class SingleSourcePolicy(SupplyChainNode fixedSource) : ISourcingPolicy
{
    public IReadOnlyList<SupplyChainNode> GetCandidateSources(
        SupplyChainNode consumer, Item item, IReadOnlyList<SupplyChainNode> available)
        => available.Contains(fixedSource) ? [fixedSource] : [];

    public IReadOnlyDictionary<SupplyChainNode, double> AllocateOrder(
        SupplyChainNode consumer, Item item, double totalQuantity,
        IReadOnlyList<SupplyChainNode> sources, TimeInstant now)
        => new Dictionary<SupplyChainNode, double> { [fixedSource] = totalQuantity };
}
