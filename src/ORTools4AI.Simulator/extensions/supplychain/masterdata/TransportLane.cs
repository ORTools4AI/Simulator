// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.extensions.supplychain.policies.transport;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;

namespace ORTools4AI.Simulator.extensions.supplychain.masterdata;

internal class TransportLane
{
    internal SupplyChainNode From { get; }
    internal SupplyChainNode To { get; }
    internal Item Item { get; }
    internal NumericalDist<double> LeadTimeDist { get; }
    internal double Capacity { get; }   // 0 = unlimited
    internal double CostPerUnit { get; }
    internal double MeanLeadTime { get; }  // used by FastestLanePolicy
    internal ITransportPolicy? PolicyOverride { get; set; }

    internal Queue<Order> ShipmentQueue { get; } = new();

    internal TransportLane(SupplyChainNode from, SupplyChainNode to, Item item,
        NumericalDist<double> leadTimeDist, double capacity, double costPerUnit,
        double meanLeadTime = 0)
    {
        From = from;
        To = to;
        Item = item;
        LeadTimeDist = leadTimeDist;
        Capacity = capacity;
        CostPerUnit = costPerUnit;
        MeanLeadTime = meanLeadTime > 0 ? meanLeadTime : 1.0;
    }
}
