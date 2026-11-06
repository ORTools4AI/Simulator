// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain;
using ORTools4AI.Simulator.extensions.supplychain.demand;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.inventory;
using ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;

using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test;

/// <summary>
/// Single-echelon inventory simulation:
///
///   Supplier (S1) ──[Exp lead time, mean 3 days]──▶ Warehouse (W1)
///                                                        ▲
///                                           Poisson demand (1 order/day, mean 10 units)
///
/// Warehouse uses a (s=50, S=200) min-max replenishment policy.
/// Simulation runs for 365 days. The test verifies the experiment
/// completes without error.
/// </summary>
public class SupplyChainExampleRun
{
    [Fact(Timeout = 120_000)]
    public void SingleEchelonInventorySimulation_Runs()
    {
        Assert.NotEqual(3, SingleEchelonModel.Run());
    }
}

/// <summary>
/// Concrete supply chain model for the single-echelon example.
/// Nodes, lanes, and policies are configured in init() so that
/// DESMOJ distributions are created after connectToExperiment().
/// </summary>
internal class SingleEchelonModel : SupplyChainModel
{
    internal SingleEchelonModel()
        : base(null, "Single-Echelon Inventory Model", true, false) { }

    public override void init()
    {
        // ── structural objects ────────────────────────────────────────────────
        var supplier  = AddNode(new Supplier("S1", "Main Supplier"));
        var warehouse = AddNode(new Warehouse("W1", "Central Warehouse"));
        var sku       = AddItem(new Item("SKU-A", "Widget A", unitCost: 10, salesPrice: 25));

        // transport lane: supplier → warehouse, exponential lead time mean 3 days
        // distributions must be created here (after connectToExperiment)
        var leadTimeDist = new ContDistExponential(this, "LT_S1_W1", 3.0, false, false);
        leadTimeDist.setNonNegative(true);

        AddLane(new TransportLane(
            from:         supplier,
            to:           warehouse,
            item:         sku,
            leadTimeDist: leadTimeDist,
            capacity:     0,
            costPerUnit:  2.0,
            meanLeadTime: 3.0));

        // ── policies ──────────────────────────────────────────────────────────

        // (s=50, S=200): replenish up to 200 units when stock falls below 50
        AttachPolicy(warehouse, sku, new MinMaxPolicy(s: 50, S: 200));

        // Poisson demand: 1 order/day, mean 10 units/order
        AttachPolicy(warehouse, sku,
            new PoissonDemandPolicy(this, arrivalRate: 1.0, meanQty: 10.0,
                name: "Demand_W1_SKU-A"));

        // always source replenishment from S1
        AttachPolicy(warehouse, new SingleSourcePolicy(supplier));

        // ── initial state ─────────────────────────────────────────────────────
        SetInitialInventory(warehouse, sku, onHand: 150);

        // create statistics (must be called after nodes and policies are registered)
        base.init();
    }

    internal static int Run()
    {
        var model = new SingleEchelonModel();
        var exp   = new Experiment("SupplyChainExperiment", TimeUnit.DAYS, TimeUnit.DAYS, null);
        model.connectToExperiment(exp);

        exp.setShowProgressBar(false);
        exp.stop(new TimeInstant(365, TimeUnit.DAYS));

        exp.start();
        int status = exp.getStatus();

        exp.report();
        exp.finish();
        return status;
    }
}
