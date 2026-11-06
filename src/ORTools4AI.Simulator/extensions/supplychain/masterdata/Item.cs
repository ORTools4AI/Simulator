// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.masterdata;

internal class Item
{
    internal string ItemId { get; }
    internal string Name { get; }
    internal double UnitCost { get; }
    internal double SalesPrice { get; }
    internal bool IsRawMaterial { get; }

    internal Item(string itemId, string name, double unitCost = 0, double salesPrice = 0, bool isRawMaterial = false)
    {
        ItemId = itemId;
        Name = name;
        UnitCost = unitCost;
        SalesPrice = salesPrice;
        IsRawMaterial = isRawMaterial;
    }

    public override string ToString() => $"{Name} ({ItemId})";
}
