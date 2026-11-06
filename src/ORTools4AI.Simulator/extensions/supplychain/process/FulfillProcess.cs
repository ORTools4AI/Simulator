// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using OrderQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.extensions.supplychain.transactionaldata.Order>;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.process;

internal class FulfillProcess : SimProcess
{
    private readonly SupplyChainNode _node;
    private readonly OrderQueue _backlogQueue = new();

    internal bool EnableBacklog { get; set; } = true;

    // statistics — keyed by item, populated by SupplyChainModel
    internal Dictionary<Item, Count> FillCount { get; } = [];
    internal Dictionary<Item, Count> TotalCount { get; } = [];
    internal Dictionary<Item, Count> LostCount { get; } = [];
    internal Dictionary<Item, Tally> LeadTimeTally { get; } = [];
    internal Dictionary<Item, Accumulate> BacklogAccum { get; } = [];

    internal FulfillProcess(Model owner, SupplyChainNode node, bool showInTrace)
        : base(owner, $"FulfillProcess-{node.NodeId}", showInTrace)
    {
        _node = node;
    }

    // Called by ArrivalEvent / ProductionCompleteEvent when a shipment lands
    internal void ReceiveDelivery(Order order)
    {
        _node.Inventory.AdjustOnHand(order.Item, order.Quantity);
        _node.Inventory.AdjustOnReceipt(order.Item, -order.Quantity);
        order.Status = OrderStatus.Fulfilled;
        // wake self to drain backlog
        if (!isScheduled())
            activate(new TimeSpan(0.0));
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            // drain backlog first with newly arrived inventory
            DrainBacklog();

            // then process new incoming customer orders
            while (_node.PendingOrders.Count > 0)
            {
                var order = _node.PendingOrders.Dequeue();
                TotalCount.GetValueOrDefault(order.Item)?.update(1);

                if (_node.Inventory.OnHand(order.Item) >= order.Quantity)
                {
                    _node.Inventory.AdjustOnHand(order.Item, -order.Quantity);
                    order.Status = OrderStatus.Fulfilled;
                    FillCount.GetValueOrDefault(order.Item)?.update(1);

                    double leadTime = presentTime().getTimeAsDouble(TimeUnit.MINUTES)
                        - order.PlacedAt.getTimeAsDouble(TimeUnit.MINUTES);
                    LeadTimeTally.GetValueOrDefault(order.Item)?.update(leadTime);
                }
                else if (EnableBacklog)
                {
                    order.Status = OrderStatus.PartiallyFilled;
                    _backlogQueue.Enqueue(order);
                    BacklogAccum.GetValueOrDefault(order.Item)?.update(_backlogQueue.Count);
                }
                else
                {
                    order.Status = OrderStatus.Lost;
                    LostCount.GetValueOrDefault(order.Item)?.update(1);
                }
            }

            // nothing to do — wait for next activation
            yield return Suspend();
        }
    }

    private void DrainBacklog()
    {
        var remaining = new OrderQueue();
        while (_backlogQueue.Count > 0)
        {
            var order = _backlogQueue.Dequeue();
            if (_node.Inventory.OnHand(order.Item) >= order.Quantity)
            {
                _node.Inventory.AdjustOnHand(order.Item, -order.Quantity);
                order.Status = OrderStatus.Fulfilled;
                FillCount.GetValueOrDefault(order.Item)?.update(1);

                double leadTime = presentTime().getTimeAsDouble(TimeUnit.MINUTES)
                    - order.PlacedAt.getTimeAsDouble(TimeUnit.MINUTES);
                LeadTimeTally.GetValueOrDefault(order.Item)?.update(leadTime);
            }
            else
            {
                remaining.Enqueue(order);
            }
        }
        while (remaining.Count > 0)
            _backlogQueue.Enqueue(remaining.Dequeue());
    }
}
