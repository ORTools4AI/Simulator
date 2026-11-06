// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.masterdata;

internal class Customer(string nodeId, string name)
    : SupplyChainNode(nodeId, name, NodeType.Customer);
