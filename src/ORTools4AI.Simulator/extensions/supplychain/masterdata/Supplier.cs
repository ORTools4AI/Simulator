// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.masterdata;

internal class Supplier(string nodeId, string name)
    : SupplyChainNode(nodeId, name, NodeType.Supplier);
