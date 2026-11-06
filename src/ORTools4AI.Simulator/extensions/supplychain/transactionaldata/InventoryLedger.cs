// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

internal class InventoryLedger
{
    private readonly Dictionary<Item, double> _onHand = [];
    private readonly Dictionary<Item, double> _onReceipt = [];

    internal double OnHand(Item item) => _onHand.GetValueOrDefault(item, 0.0);
    internal double OnReceipt(Item item) => _onReceipt.GetValueOrDefault(item, 0.0);

    internal void AdjustOnHand(Item item, double delta)
    {
        _onHand[item] = OnHand(item) + delta;
    }

    internal void AdjustOnReceipt(Item item, double delta)
    {
        _onReceipt[item] = OnReceipt(item) + delta;
    }

    internal IEnumerable<Item> TrackedItems()
    {
        var keys = new HashSet<Item>(_onHand.Keys);
        keys.UnionWith(_onReceipt.Keys);
        return keys;
    }
}
