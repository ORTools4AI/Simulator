// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;

// Try sources in priority order; move to next if first has insufficient inventory
internal class PrioritySourcePolicy(IReadOnlyList<SupplyChainNode> priorityList) : ISourcingPolicy
{
    public IReadOnlyList<SupplyChainNode> GetCandidateSources(
        SupplyChainNode consumer, Item item, IReadOnlyList<SupplyChainNode> available)
        => priorityList.Where(s => available.Contains(s)).ToList();

    public IReadOnlyDictionary<SupplyChainNode, double> AllocateOrder(
        SupplyChainNode consumer, Item item, double totalQuantity,
        IReadOnlyList<SupplyChainNode> sources, TimeInstant now)
    {
        var result = new Dictionary<SupplyChainNode, double>();
        double remaining = totalQuantity;
        foreach (var source in sources)
        {
            if (remaining <= 0) break;
            double available = source.Inventory.OnHand(item);
            double take = Math.Min(remaining, available);
            if (take > 0)
            {
                result[source] = take;
                remaining -= take;
            }
        }
        // if still remaining, assign to first source regardless
        if (remaining > 0 && sources.Count > 0)
            result[sources[0]] = result.GetValueOrDefault(sources[0], 0) + remaining;
        return result;
    }
}
