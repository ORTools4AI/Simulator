// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.extensions.supplychain.policies.transport;

internal interface ITransportPolicy
{
    TransportLane SelectLane(SupplyChainNode origin, SupplyChainNode destination,
        Item item, IReadOnlyList<TransportLane> candidates);

    double DecideShipmentQuantity(TransportLane lane, Order order);
}
