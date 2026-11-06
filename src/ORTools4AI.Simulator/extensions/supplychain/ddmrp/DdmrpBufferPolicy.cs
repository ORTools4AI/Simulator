// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.inventory;

namespace ORTools4AI.Simulator.extensions.supplychain.ddmrp;

// IInventoryPolicy implementation for DDMRP replenishment.
//   ShouldReview      → true when NFP < TopOfRed
//   DecideOrderQty    → TopOfGreen - NFP  (order-up-to)
// NFP = OnHand + OnOrder - QualifiedDemandSpikes
internal class DdmrpBufferPolicy(DdmrpBuffer buffer,
                                  DdmrpDemandSpikeTracker? spikes = null) : IInventoryPolicy
{
    public bool ShouldReview(SupplyChainNode node, Item item, TimeInstant now)
        => NetFlowPosition(node, item, now) < buffer.TopOfRed;

    public double DecideOrderQuantity(SupplyChainNode node, Item item, TimeInstant now)
        => Math.Max(0.0, buffer.TopOfGreen - NetFlowPosition(node, item, now));

    private double NetFlowPosition(SupplyChainNode node, Item item, TimeInstant now)
    {
        double qds = spikes?.QualifiedDemandSpikes(now.getTimeAsDouble(TimeUnit.DAYS)) ?? 0.0;
        return node.Inventory.OnHand(item) + node.Inventory.OnReceipt(item) - qds;
    }
}
