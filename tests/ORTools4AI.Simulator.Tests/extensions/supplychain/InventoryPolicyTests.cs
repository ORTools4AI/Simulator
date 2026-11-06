// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.inventory;
using ORTools4AI.Simulator.Tests.Helpers;

namespace ORTools4AI.Simulator.Tests.extensions.supplychain;

// Shared helpers for policy tests
internal static class PolicyTestHelpers
{
    internal static SupplyChainNode MakeWarehouse(string id = "W1")
        => new SupplyChainNode(id, $"Warehouse {id}", NodeType.Warehouse);

    internal static Item MakeItem(string id = "SKU-1")
        => new Item(id, $"Item {id}");
}

// ---------------------------------------------------------------------------
// MinMaxPolicy tests
// ---------------------------------------------------------------------------

public class MinMaxPolicyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _node;
    private readonly Item _item;

    public MinMaxPolicyTests()
    {
        (_,  _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _node = PolicyTestHelpers.MakeWarehouse();
        _item = PolicyTestHelpers.MakeItem();
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void ShouldReview_OnHandBelowS_ReturnsTrue()
    {
        var policy = new MinMaxPolicy(s: 50, S: 200);
        _node.Inventory.AdjustOnHand(_item, 30.0); // 30 < 50
        Assert.True(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void ShouldReview_OnHandEqualToS_ReturnsFalse()
    {
        var policy = new MinMaxPolicy(s: 50, S: 200);
        _node.Inventory.AdjustOnHand(_item, 50.0); // 50 not < 50
        Assert.False(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void ShouldReview_OnHandAboveS_ReturnsFalse()
    {
        var policy = new MinMaxPolicy(s: 50, S: 200);
        _node.Inventory.AdjustOnHand(_item, 75.0);
        Assert.False(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void DecideOrderQuantity_NoOnHand_OrdersUpToS()
    {
        var policy = new MinMaxPolicy(s: 50, S: 200);
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.Equal(200.0, qty);
    }

    [Fact]
    public void DecideOrderQuantity_PartialOnHand_OrdersRemainder()
    {
        var policy = new MinMaxPolicy(s: 50, S: 200);
        _node.Inventory.AdjustOnHand(_item, 80.0);
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.Equal(120.0, qty, precision: 6);
    }

    [Fact]
    public void DecideOrderQuantity_OnHandPlusOnReceiptCoversS_ReturnsZero()
    {
        var policy = new MinMaxPolicy(s: 50, S: 200);
        _node.Inventory.AdjustOnHand(_item, 100.0);
        _node.Inventory.AdjustOnReceipt(_item, 150.0); // 100+150=250 > 200
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.Equal(0.0, qty, precision: 6);
    }
}

// ---------------------------------------------------------------------------
// FixedOrderQuantityPolicy tests
// ---------------------------------------------------------------------------

public class FixedOrderQuantityPolicyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _node;
    private readonly Item _item;

    public FixedOrderQuantityPolicyTests()
    {
        (_, _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _node = PolicyTestHelpers.MakeWarehouse();
        _item = PolicyTestHelpers.MakeItem();
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void ShouldReview_OnHandEqualToR_ReturnsTrue()
    {
        var policy = new FixedOrderQuantityPolicy(r: 20, Q: 100);
        _node.Inventory.AdjustOnHand(_item, 20.0); // 20 <= 20
        Assert.True(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void ShouldReview_OnHandBelowR_ReturnsTrue()
    {
        var policy = new FixedOrderQuantityPolicy(r: 20, Q: 100);
        _node.Inventory.AdjustOnHand(_item, 10.0);
        Assert.True(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void ShouldReview_OnHandAboveR_ReturnsFalse()
    {
        var policy = new FixedOrderQuantityPolicy(r: 20, Q: 100);
        _node.Inventory.AdjustOnHand(_item, 21.0);
        Assert.False(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void DecideOrderQuantity_AlwaysReturnsQ()
    {
        var policy = new FixedOrderQuantityPolicy(r: 20, Q: 100);
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.Equal(100.0, qty);
    }

    [Fact]
    public void DecideOrderQuantity_DespiteHighOnHand_StillReturnsQ()
    {
        var policy = new FixedOrderQuantityPolicy(r: 20, Q: 100);
        _node.Inventory.AdjustOnHand(_item, 500.0);
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.Equal(100.0, qty);
    }
}

// ---------------------------------------------------------------------------
// BaseStockPolicy tests
// ---------------------------------------------------------------------------

public class BaseStockPolicyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _node;
    private readonly Item _item;

    public BaseStockPolicyTests()
    {
        (_, _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _node = PolicyTestHelpers.MakeWarehouse();
        _item = PolicyTestHelpers.MakeItem();
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void ShouldReview_Always_ReturnsTrue()
    {
        var policy = new BaseStockPolicy(B: 200);
        _node.Inventory.AdjustOnHand(_item, 1000.0);
        Assert.True(policy.ShouldReview(_node, _item, _now));
    }

    [Fact]
    public void DecideOrderQuantity_NoOnHand_OrdersB()
    {
        var policy = new BaseStockPolicy(B: 200);
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.True(qty > 0);
    }

    [Fact]
    public void DecideOrderQuantity_OnHandCoversB_ReturnsZeroOrPositive()
    {
        var policy = new BaseStockPolicy(B: 100);
        _node.Inventory.AdjustOnHand(_item, 100.0);
        _node.Inventory.AdjustOnReceipt(_item, 50.0);
        double qty = policy.DecideOrderQuantity(_node, _item, _now);
        Assert.Equal(0.0, qty, precision: 6);
    }
}
