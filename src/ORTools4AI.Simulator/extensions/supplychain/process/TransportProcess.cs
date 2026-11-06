// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.transport;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.process;

internal class TransportProcess : SimProcess
{
    private readonly TransportLane _lane;
    private readonly FulfillProcess _destFulfillProcess;
    private static readonly DirectShipPolicy _defaultPolicy = new();

    internal Count? ThroughputCount { get; set; }

    internal TransportProcess(Model owner, TransportLane lane,
        FulfillProcess destFulfillProcess, bool showInTrace)
        : base(owner, $"TransportProcess-{lane.From.NodeId}-{lane.To.NodeId}-{lane.Item.ItemId}",
            showInTrace)
    {
        _lane = lane;
        _destFulfillProcess = destFulfillProcess;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            while (_lane.ShipmentQueue.Count == 0)
                yield return Suspend();

            var order = _lane.ShipmentQueue.Dequeue();
            var policy = _lane.PolicyOverride ?? order.Origin.TransportPolicy ?? _defaultPolicy;
            double qty = policy.DecideShipmentQuantity(_lane, order);
            double leadTime = _lane.LeadTimeDist.sample();

            order.Status = OrderStatus.InTransit;
            ThroughputCount?.update(1);

            // schedule arrival after lead time
            var arrival = new ArrivalEvent(getModel(), order, qty, _destFulfillProcess);
            arrival.schedule(new TimeSpan(leadTime));
        }
    }
}

internal class ArrivalEvent : ExternalEvent
{
    private readonly Order _order;
    private readonly double _qty;
    private readonly FulfillProcess _fulfillProcess;

    internal ArrivalEvent(Model owner, Order order, double qty, FulfillProcess fulfillProcess)
        : base(owner, "ArrivalEvent", false)
    {
        _order = order;
        _qty = qty;
        _fulfillProcess = fulfillProcess;
    }

    public override void eventRoutine()
    {
        _fulfillProcess.ReceiveDelivery(_order);
    }
}
