// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;

namespace ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

internal enum OrderType { CustomerOrder, ReplenishmentOrder, ProductionOrder }
internal enum OrderStatus { Created, InTransit, PartiallyFilled, Fulfilled, Lost }

internal class Order : Entity
{
    internal string OrderId { get; }
    internal Item Item { get; }
    internal double Quantity { get; }
    internal SupplyChainNode Origin { get; }
    internal SupplyChainNode Destination { get; }
    internal OrderType Type { get; }
    internal OrderStatus Status { get; set; }
    internal TimeInstant PlacedAt { get; }
    internal TimeInstant? DueAt { get; set; }

    internal Order(Model owner, string orderId, Item item, double quantity,
        SupplyChainNode origin, SupplyChainNode destination,
        OrderType type, TimeInstant placedAt, bool showInTrace = false)
        : base(owner, orderId, showInTrace)
    {
        OrderId = orderId;
        Item = item;
        Quantity = quantity;
        Origin = origin;
        Destination = destination;
        Type = type;
        PlacedAt = placedAt;
        Status = OrderStatus.Created;
    }
}
