// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.transport;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using ORTools4AI.Simulator.Tests.Helpers;

namespace ORTools4AI.Simulator.Tests.extensions.supplychain;

public class TransportPolicyTests : IDisposable
{
    private readonly HarnessModel _model;
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _w1;
    private readonly SupplyChainNode _r1;
    private readonly SupplyChainNode _r2;
    private readonly Item _item;

    public TransportPolicyTests()
    {
        (_model, _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _w1  = new SupplyChainNode("W1", "Warehouse", NodeType.Warehouse);
        _r1  = new SupplyChainNode("R1", "Retailer1", NodeType.Retailer);
        _r2  = new SupplyChainNode("R2", "Retailer2", NodeType.Retailer);
        _item = new Item("SKU-1", "Widget");
    }

    public void Dispose() => _exp.finish();

    private TransportLane MakeLane(SupplyChainNode from, SupplyChainNode to,
        double costPerUnit, double meanLeadTime, double capacity = 0)
    {
        var dist = new ContDistExponential(_model, $"LT_{from.NodeId}_{to.NodeId}",
            meanLeadTime, false, false);
        dist.setNonNegative(true);
        return new TransportLane(from, to, _item, dist, capacity, costPerUnit, meanLeadTime);
    }

    private Order MakeOrder(double qty) =>
        new Order(_model, "ORD-1", _item, qty, _w1, _r1,
            OrderType.ReplenishmentOrder, _now);

    // ── CheapestLanePolicy ───────────────────────────────────────────────────

    [Fact]
    public void CheapestLane_SelectsLaneWithLowestCost()
    {
        var cheap = MakeLane(_w1, _r1, costPerUnit: 1.0, meanLeadTime: 2.0);
        var pricey = MakeLane(_w1, _r2, costPerUnit: 5.0, meanLeadTime: 1.0);
        var policy = new CheapestLanePolicy();
        var selected = policy.SelectLane(_w1, _r1, _item, [cheap, pricey]);
        Assert.Equal(cheap, selected);
    }

    [Fact]
    public void CheapestLane_SingleCandidate_ReturnsThat()
    {
        var lane = MakeLane(_w1, _r1, costPerUnit: 3.0, meanLeadTime: 1.0);
        var policy = new CheapestLanePolicy();
        Assert.Equal(lane, policy.SelectLane(_w1, _r1, _item, [lane]));
    }

    [Fact]
    public void CheapestLane_UnlimitedCapacity_DecidesFull()
    {
        var lane = MakeLane(_w1, _r1, costPerUnit: 1.0, meanLeadTime: 1.0, capacity: 0);
        var order = MakeOrder(50.0);
        var policy = new CheapestLanePolicy();
        Assert.Equal(50.0, policy.DecideShipmentQuantity(lane, order), precision: 6);
    }

    [Fact]
    public void CheapestLane_LimitedCapacity_CapsAtCapacity()
    {
        var lane = MakeLane(_w1, _r1, costPerUnit: 1.0, meanLeadTime: 1.0, capacity: 30);
        var order = MakeOrder(50.0);
        var policy = new CheapestLanePolicy();
        Assert.Equal(30.0, policy.DecideShipmentQuantity(lane, order), precision: 6);
    }

    [Fact]
    public void CheapestLane_LimitedCapacityLargerThanOrder_ShipsFullOrder()
    {
        var lane = MakeLane(_w1, _r1, costPerUnit: 1.0, meanLeadTime: 1.0, capacity: 100);
        var order = MakeOrder(50.0);
        var policy = new CheapestLanePolicy();
        Assert.Equal(50.0, policy.DecideShipmentQuantity(lane, order), precision: 6);
    }

    // ── FastestLanePolicy ────────────────────────────────────────────────────

    [Fact]
    public void FastestLane_SelectsLaneWithLowestMeanLeadTime()
    {
        var fast = MakeLane(_w1, _r1, costPerUnit: 5.0, meanLeadTime: 0.5);
        var slow = MakeLane(_w1, _r2, costPerUnit: 1.0, meanLeadTime: 3.0);
        var policy = new FastestLanePolicy();
        var selected = policy.SelectLane(_w1, _r1, _item, [fast, slow]);
        Assert.Equal(fast, selected);
    }

    [Fact]
    public void FastestLane_SingleCandidate_ReturnsThat()
    {
        var lane = MakeLane(_w1, _r1, costPerUnit: 2.0, meanLeadTime: 1.0);
        var policy = new FastestLanePolicy();
        Assert.Equal(lane, policy.SelectLane(_w1, _r1, _item, [lane]));
    }

    // ── DirectShipPolicy ─────────────────────────────────────────────────────

    [Fact]
    public void DirectShip_SelectsFirstCandidate()
    {
        var first  = MakeLane(_w1, _r1, costPerUnit: 3.0, meanLeadTime: 1.0);
        var second = MakeLane(_w1, _r2, costPerUnit: 1.0, meanLeadTime: 0.5);
        var policy = new DirectShipPolicy();
        var selected = policy.SelectLane(_w1, _r1, _item, [first, second]);
        Assert.Equal(first, selected);
    }
}
