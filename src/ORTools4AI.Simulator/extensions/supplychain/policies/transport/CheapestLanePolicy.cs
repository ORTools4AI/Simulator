// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.transport;

internal class CheapestLanePolicy : ITransportPolicy
{
    public TransportLane SelectLane(SupplyChainNode origin, SupplyChainNode destination,
        Item item, IReadOnlyList<TransportLane> candidates)
        => candidates.MinBy(l => l.CostPerUnit)!;

    public double DecideShipmentQuantity(TransportLane lane, Order order)
        => lane.Capacity > 0 ? Math.Min(order.Quantity, lane.Capacity) : order.Quantity;
}
