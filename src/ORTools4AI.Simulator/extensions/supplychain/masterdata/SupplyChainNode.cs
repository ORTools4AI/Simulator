// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.extensions.supplychain.demand;
using ORTools4AI.Simulator.extensions.supplychain.policies.inventory;
using ORTools4AI.Simulator.extensions.supplychain.policies.production;
using ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;
using ORTools4AI.Simulator.extensions.supplychain.policies.transport;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.extensions.supplychain.masterdata;

internal enum NodeType { Supplier, Plant, Warehouse, Retailer, Customer }

internal class SupplyChainNode
{
    internal string NodeId { get; }
    internal string Name { get; }
    internal NodeType Type { get; }

    internal Dictionary<Item, IInventoryPolicy> InventoryPolicies { get; } = [];
    internal Dictionary<Item, IManufacturingPolicy> ManufacturingPolicies { get; } = [];
    internal Dictionary<Item, IDemandPolicy> DemandPolicies { get; } = [];

    internal ITransportPolicy? TransportPolicy { get; set; }
    internal ISourcingPolicy? SourcingPolicy { get; set; }

    internal InventoryLedger Inventory { get; } = new();
    internal Queue<Order> PendingOrders { get; } = new();

    internal SupplyChainNode(string nodeId, string name, NodeType type)
    {
        NodeId = nodeId;
        Name = name;
        Type = type;
    }

    public override string ToString() => $"{Name} ({NodeId})";
}
