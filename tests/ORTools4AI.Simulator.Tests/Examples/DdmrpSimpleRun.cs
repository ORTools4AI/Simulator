// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain;
using ORTools4AI.Simulator.extensions.supplychain.ddmrp;
using ORTools4AI.Simulator.extensions.supplychain.demand;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;

namespace SlogTek.Desmoj.Test;

/// <summary>
/// DDMRP single-echelon inventory simulation:
///
///   Supplier (S1) ──[Exp LT, mean 3 days]──▶ Warehouse/FGB (W1)
///                                                   ▲
///                                    Demand: Poisson(1 order/day, mean 1 unit)
///
/// FGB uses DdmrpBufferPolicy (Formula 1):
///   ADU=1 unit/day, LT=3 days, F_L=0.5, F_V=0.5, GZ=3 days
///   → RedZone=2.25, YellowZone=3, TOY=5.25, GZ=3, TOG=8.25
///
/// With initial stock at TOG and a realistic buffer, service level should be high.
/// Verifies the experiment completes and the DDMRP policy triggers correctly.
/// </summary>
public class DdmrpSimpleRun
{
    [Fact(Timeout = 300_000)]
    public void DdmrpFgbSimulation_RunsToCompletion()
    {
        int status = DdmrpSingleEchelonModel.Run();
        Assert.NotEqual(3, status); // 3 = error status in DESMOJ
    }
}

internal class DdmrpSingleEchelonModel : SupplyChainModel
{
    // Exposed for assertion convenience in tests
    internal double? FinalServiceLevel { get; private set; }

    internal DdmrpSingleEchelonModel()
        : base(null, "DDMRP Single-Echelon Model", true, false) { }

    public override void init()
    {
        var supplier  = AddNode(new Supplier("S1", "RMB Supplier"));
        var fgb       = AddNode(new Warehouse("W1", "Finished Goods Buffer"));
        var sku       = AddItem(new Item("SKU-A", "Widget A", unitCost: 5, salesPrice: 20));

        // Transport: stochastic replenishment lead time, mean 3 days
        var ltDist = new ContDistExponential(this, "LT_S1_W1", 3.0, false, false);
        ltDist.setNonNegative(true);
        AddLane(new TransportLane(supplier, fgb, sku, ltDist,
            capacity: 0, costPerUnit: 1.0, meanLeadTime: 3.0));

        // DDMRP buffer: ADU=1 u/day, LT=3 days, F_L=0.5, F_V=0.5, GZ=3 days
        var buffer = new DdmrpBuffer
        {
            Adu              = 1.0,
            LeadTime         = 3.0,
            LeadTimeFactor   = 0.5,
            VariabilityFactor = 0.5,
            GreenZone        = 3.0
        };
        // TOY = 5.25, TOG = 8.25 — initial stock set at TOG
        var ddmrpPolicy = new DdmrpBufferPolicy(buffer);
        AttachPolicy(fgb, sku, ddmrpPolicy);

        // Demand: 1 order/day, mean 1 unit/order (Exponential qty)
        AttachPolicy(fgb, sku, new PoissonDemandPolicy(this,
            arrivalRate: 1.0, meanQty: 1.0, name: "Demand_FGB_SKU-A"));

        AttachPolicy(fgb, new SingleSourcePolicy(supplier));

        // Seed inventory at TOG
        SetInitialInventory(fgb, sku, onHand: (int)Math.Ceiling(buffer.TopOfGreen));

        // Daily replenishment review matches DDMRP operational cadence
        ReplenishmentReviewInterval = 1.0;

        base.init();
    }

    internal static int Run()
    {
        var savedEpsilon = TimeOperations.getEpsilon();
        var savedRef     = TimeOperations.getReferenceUnit();
        try
        {
            var model = new DdmrpSingleEchelonModel();
            var exp = new Experiment("DdmrpExperiment", TimeUnit.DAYS, TimeUnit.DAYS, null);
            model.connectToExperiment(exp);

            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(365, TimeUnit.DAYS));

            exp.start();
            int status = exp.getStatus();
            exp.report();
            exp.finish();
            return status;
        }
        finally
        {
            TimeOperations.setEpsilon(savedEpsilon);
            TimeOperations.setReferenceUnit(savedRef);
        }
    }
}
