// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.process;

internal class ReplenishmentProcess : SimProcess
{
    private readonly SupplyChainNode _node;
    private readonly double _reviewInterval;
    private readonly IReadOnlyList<SupplyChainNode> _upstreamNodes;
    private readonly IReadOnlyDictionary<TransportLane, TransportProcess> _laneProcesses;
    private int _orderCounter;

    internal ReplenishmentProcess(Model owner, SupplyChainNode node,
        double reviewInterval, IReadOnlyList<SupplyChainNode> upstreamNodes,
        IReadOnlyDictionary<TransportLane, TransportProcess> laneProcesses,
        bool showInTrace)
        : base(owner, $"ReplenProcess-{node.NodeId}", showInTrace)
    {
        _node = node;
        _reviewInterval = reviewInterval;
        _upstreamNodes = upstreamNodes;
        _laneProcesses = laneProcesses;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            yield return Delay(_reviewInterval);

            foreach (var (item, policy) in _node.InventoryPolicies)
            {
                if (!policy.ShouldReview(_node, item, presentTime())) continue;
                double qty = policy.DecideOrderQuantity(_node, item, presentTime());
                if (qty <= 0) continue;

                var sources = _node.SourcingPolicy!.GetCandidateSources(
                    _node, item, _upstreamNodes);
                var allocation = _node.SourcingPolicy.AllocateOrder(
                    _node, item, qty, sources, presentTime());

                foreach (var (source, partialQty) in allocation)
                {
                    if (partialQty <= 0) continue;
                    var lane = FindLane(source, _node, item);
                    if (lane is null) continue;

                    var order = new Order(getModel(),
                        $"REPLEN-{_node.NodeId}-{item.ItemId}-{++_orderCounter}",
                        item, partialQty, source, _node,
                        OrderType.ReplenishmentOrder, presentTime());

                    _node.Inventory.AdjustOnReceipt(item, partialQty);
                    lane.ShipmentQueue.Enqueue(order);

                    if (_laneProcesses.TryGetValue(lane, out var tp) && !tp.isScheduled())
                        tp.activate(new TimeSpan(0.0));
                }
            }
        }
    }

    private TransportLane? FindLane(SupplyChainNode from, SupplyChainNode to, Item item)
    {
        foreach (var (lane, _) in _laneProcesses)
            if (lane.From == from && lane.To == to && lane.Item == item)
                return lane;
        return null;
    }
}
