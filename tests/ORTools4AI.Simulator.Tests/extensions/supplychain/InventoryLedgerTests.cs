// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.Tests.extensions.supplychain;

public class InventoryLedgerTests
{
    private static Item MakeItem(string id = "SKU-1") => new Item(id, $"Item {id}");

    [Fact]
    public void OnHand_NewItem_ReturnsZero()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        Assert.Equal(0.0, ledger.OnHand(item));
    }

    [Fact]
    public void OnReceipt_NewItem_ReturnsZero()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        Assert.Equal(0.0, ledger.OnReceipt(item));
    }

    [Fact]
    public void AdjustOnHand_PositiveDelta_IncreasesOnHand()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnHand(item, 50.0);
        Assert.Equal(50.0, ledger.OnHand(item));
    }

    [Fact]
    public void AdjustOnHand_NegativeDelta_DecreasesOnHand()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnHand(item, 100.0);
        ledger.AdjustOnHand(item, -30.0);
        Assert.Equal(70.0, ledger.OnHand(item));
    }

    [Fact]
    public void AdjustOnReceipt_PositiveDelta_IncreasesOnReceipt()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnReceipt(item, 20.0);
        Assert.Equal(20.0, ledger.OnReceipt(item));
    }

    [Fact]
    public void AdjustOnReceipt_NegativeDelta_DecreasesOnReceipt()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnReceipt(item, 40.0);
        ledger.AdjustOnReceipt(item, -15.0);
        Assert.Equal(25.0, ledger.OnReceipt(item));
    }

    [Fact]
    public void MultipleAdjustments_Accumulate()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnHand(item, 10.0);
        ledger.AdjustOnHand(item, 20.0);
        ledger.AdjustOnHand(item, 5.0);
        Assert.Equal(35.0, ledger.OnHand(item));
    }

    [Fact]
    public void TwoItems_TrackIndependently()
    {
        var ledger = new InventoryLedger();
        var a = MakeItem("A");
        var b = MakeItem("B");
        ledger.AdjustOnHand(a, 100.0);
        ledger.AdjustOnHand(b, 200.0);
        Assert.Equal(100.0, ledger.OnHand(a));
        Assert.Equal(200.0, ledger.OnHand(b));
    }

    [Fact]
    public void TrackedItems_IncludesOnHandItems()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnHand(item, 10.0);
        Assert.Contains(item, ledger.TrackedItems());
    }

    [Fact]
    public void TrackedItems_IncludesOnReceiptItems()
    {
        var ledger = new InventoryLedger();
        var item = MakeItem();
        ledger.AdjustOnReceipt(item, 5.0);
        Assert.Contains(item, ledger.TrackedItems());
    }

    [Fact]
    public void TrackedItems_EmptyLedger_IsEmpty()
    {
        var ledger = new InventoryLedger();
        Assert.Empty(ledger.TrackedItems());
    }
}
