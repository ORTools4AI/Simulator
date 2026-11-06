// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.demand;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.process;

internal class DemandProcess : SimProcess
{
    private readonly SupplyChainNode _node;
    private readonly Item _item;
    private readonly IDemandPolicy _policy;
    private readonly FulfillProcess _fulfillProcess;
    private int _orderCounter;

    // Optional hook: called with (simulationTimeDays, orderQty) on every demand arrival.
    // Wire to DdmrpDemandSpikeTracker.RecordDemand or DdmrpBufferAdjuster.RecordDemand.
    internal Action<double, double>? OnDemandArrived { get; set; }

    internal DemandProcess(Model owner, SupplyChainNode node, Item item,
        IDemandPolicy policy, FulfillProcess fulfillProcess, bool showInTrace)
        : base(owner, $"DemandProcess-{node.NodeId}-{item.ItemId}", showInTrace)
    {
        _node = node;
        _item = item;
        _policy = policy;
        _fulfillProcess = fulfillProcess;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            double interArrival = _policy.InterArrivalTimeDist.sample();
            yield return Delay(interArrival);

            double qty = Math.Max(1, _policy.OrderQuantityDist.sample());
            OnDemandArrived?.Invoke(presentTime().getTimeAsDouble(TimeUnit.DAYS), qty);
            var order = new Order(getModel(),
                $"ORD-{_node.NodeId}-{_item.ItemId}-{++_orderCounter}",
                _item, qty, _node, _node, OrderType.CustomerOrder,
                presentTime());

            _node.PendingOrders.Enqueue(order);
            if (!_fulfillProcess.isScheduled())
                _fulfillProcess.activate(new TimeSpan(0.0));
        }
    }
}
