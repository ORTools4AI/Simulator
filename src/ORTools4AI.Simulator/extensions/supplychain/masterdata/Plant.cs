// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.masterdata;

internal class Plant(string nodeId, string name)
    : SupplyChainNode(nodeId, name, NodeType.Plant);
