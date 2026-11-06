// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.production;

namespace ORTools4AI.Simulator.extensions.supplychain.ddmrp;

// IManufacturingPolicy implementation for DDMRP-driven production.
// Triggers production when NFP < TopOfRed; batch size = TopOfGreen − NFP.
internal class DdmrpProductionPolicy(
    DdmrpBuffer buffer,
    NumericalDist<double> leadTimeDist,
    IReadOnlyDictionary<Item, double>? bomPer1Unit = null,
    DdmrpDemandSpikeTracker? spikes = null) : IManufacturingPolicy
{
    private readonly IReadOnlyDictionary<Item, double> _bom =
        bomPer1Unit ?? new Dictionary<Item, double>();

    public ManufacturingMode Mode => ManufacturingMode.MakeToStock;

    public double DecideProductionQuantity(Plant plant, Item finishedGood, TimeInstant now)
    {
        double nfp = NetFlowPosition(plant, finishedGood, now);
        return nfp < buffer.TopOfRed ? Math.Max(0.0, buffer.TopOfGreen - nfp) : 0.0;
    }

    public IReadOnlyDictionary<Item, double> GetBomRequirements(Item finishedGood, double qty)
        => _bom.ToDictionary(kv => kv.Key, kv => kv.Value * qty);

    public NumericalDist<double> ProductionLeadTimeDist(Plant plant, Item finishedGood)
        => leadTimeDist;

    private double NetFlowPosition(Plant plant, Item item, TimeInstant now)
    {
        double qds = spikes?.QualifiedDemandSpikes(now.getTimeAsDouble(TimeUnit.DAYS)) ?? 0.0;
        return plant.Inventory.OnHand(item) + plant.Inventory.OnReceipt(item) - qds;
    }
}
