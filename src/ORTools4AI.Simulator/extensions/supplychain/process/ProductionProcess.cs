// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.process;

internal class ProductionProcess : SimProcess
{
    private readonly Plant _plant;
    private readonly double _reviewInterval;
    private readonly FulfillProcess _fulfillProcess;
    private int _orderCounter;

    internal ProductionProcess(Model owner, Plant plant, double reviewInterval,
        FulfillProcess fulfillProcess, bool showInTrace)
        : base(owner, $"ProductionProcess-{plant.NodeId}", showInTrace)
    {
        _plant = plant;
        _reviewInterval = reviewInterval;
        _fulfillProcess = fulfillProcess;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            yield return Delay(_reviewInterval);

            foreach (var (item, policy) in _plant.ManufacturingPolicies)
            {
                double qty = policy.DecideProductionQuantity(_plant, item, presentTime());
                if (qty <= 0) continue;

                var bom = policy.GetBomRequirements(item, qty);

                // check all raw materials are available
                bool canProduce = true;
                foreach (var (rawMat, required) in bom)
                {
                    if (_plant.Inventory.OnHand(rawMat) < required)
                    {
                        canProduce = false;
                        break;
                    }
                }
                if (!canProduce) continue;

                // deduct raw materials
                foreach (var (rawMat, required) in bom)
                    _plant.Inventory.AdjustOnHand(rawMat, -required);

                double leadTime = policy.ProductionLeadTimeDist(_plant, item).sample();
                var order = new Order(getModel(),
                    $"PROD-{_plant.NodeId}-{item.ItemId}-{++_orderCounter}",
                    item, qty, _plant, _plant,
                    OrderType.ProductionOrder, presentTime());

                // schedule production completion
                var done = new ProductionCompleteEvent(getModel(), order, _fulfillProcess);
                done.schedule(new TimeSpan(leadTime));
            }
        }
    }
}

internal class ProductionCompleteEvent : ExternalEvent
{
    private readonly Order _order;
    private readonly FulfillProcess _fulfillProcess;

    internal ProductionCompleteEvent(Model owner, Order order, FulfillProcess fulfillProcess)
        : base(owner, "ProductionCompleteEvent", false)
    {
        _order = order;
        _fulfillProcess = fulfillProcess;
    }

    public override void eventRoutine()
    {
        _fulfillProcess.ReceiveDelivery(_order);
    }
}
