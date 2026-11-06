// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using JobQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.extensions.supplychain.process.ProductionJob>;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.process;

// A job routed through a CapacityStationProcess.
// OnComplete is called with the finished Order once the station completes processing.
internal sealed class ProductionJob(
    Item item, double qty, SupplyChainNode origin, SupplyChainNode destination,
    TimeInstant placedAt, Action<Order> onComplete)
{
    internal Item Item { get; } = item;
    internal double Quantity { get; } = qty;
    internal SupplyChainNode Origin { get; } = origin;
    internal SupplyChainNode Destination { get; } = destination;
    internal TimeInstant PlacedAt { get; } = placedAt;
    internal Action<Order> OnComplete { get; } = onComplete;
}

// Single-capacity (one-machine) manufacturing station.
// Jobs queue FCFS; each job occupies the station for stochastic setup + processing time.
// On completion, OnComplete is invoked — typically enqueuing the next station or
// calling FulfillProcess.ReceiveDelivery for final-stage delivery.
internal class CapacityStationProcess : SimProcess
{
    private readonly SupplyChainNode _station;
    private readonly NumericalDist<double> _processingTimeDist;
    private readonly NumericalDist<double>? _setupTimeDist;
    private int _jobCounter;

    internal JobQueue JobQueue { get; } = new();

    internal CapacityStationProcess(Model owner, SupplyChainNode station,
        NumericalDist<double> processingTimeDist,
        NumericalDist<double>? setupTimeDist = null,
        bool showInTrace = false)
        : base(owner, $"Station-{station.NodeId}", showInTrace)
    {
        _station = station;
        _processingTimeDist = processingTimeDist;
        _setupTimeDist = setupTimeDist;
    }

    internal void Enqueue(ProductionJob job)
    {
        JobQueue.Enqueue(job);
        if (!isScheduled())
            activate(new TimeSpan(0.0));
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            if (JobQueue.Count == 0)
            {
                // No self-scheduled wakeup needed: Enqueue() calls activate(new TimeSpan(0.0))
                // when it adds the first job, guaranteeing this process is rescheduled.
                yield return Suspend();
                continue;
            }

            var job = JobQueue.Dequeue();

            if (_setupTimeDist is not null)
            {
                yield return Delay(_setupTimeDist.sample());
            }

            yield return Delay(_processingTimeDist.sample());

            var order = new Order(getModel(),
                $"STATION-{_station.NodeId}-{job.Item.ItemId}-{++_jobCounter}",
                job.Item, job.Quantity, job.Origin, job.Destination,
                OrderType.ProductionOrder, job.PlacedAt);
            job.OnComplete(order);
        }
    }
}
