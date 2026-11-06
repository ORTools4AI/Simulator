// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain;
using ORTools4AI.Simulator.extensions.supplychain.demand;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.inventory;
using ORTools4AI.Simulator.extensions.supplychain.policies.production;
using ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;
using ORTools4AI.Simulator.extensions.supplychain.policies.transport;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test;

/// <summary>
/// Three-echelon supply chain with manufacturing, covering all five simulation
/// processes (FulfillProcess, TransportProcess, DemandProcess, ReplenishmentProcess,
/// ProductionProcess) and a variety of policy classes.
///
/// Network topology:
///
///   S1 (Supplier) ─[Exp 2d]─▶
///                              P1 (Plant) ─[Exp 1d]─▶ W1 (Warehouse) ─[Exp 0.5d]─▶ R1 (Poisson demand)
///   S2 (Supplier) ─[Exp 4d]─▶              ─[Exp 2d]─▶ W2 (Warehouse) ─[Exp 0.5d]─▶ R2 (periodic demand)
///                                                                         ─[Exp 0.5d]─▶ R3 (trace demand)
///                                           W1 ─[Exp 0.5d]─▶ R3 (dual-sourced from W1+W2)
///
/// Processes exercised:
///   FulfillProcess          — all inventory-holding nodes (P1, W1, W2, R1, R2, R3)
///   TransportProcess        — all 8 transport lanes
///   DemandProcess           — R1 (Poisson), R2 (Periodic), R3 (TraceDriven)
///   ReplenishmentProcess    — P1 (raw mat), W1 (widget), W2 (widget), R1, R2, R3
///   ProductionProcess       — P1 with MakeToStockPolicy
///
/// Policies exercised:
///   Inventory  : MinMaxPolicy (R1, W2), FixedOrderQuantityPolicy (R2), BaseStockPolicy (W1, R3)
///   Demand     : PoissonDemandPolicy (R1), PeriodicDemandPolicy (R2), TraceDrivenDemandPolicy (R3)
///   Sourcing   : SingleSourcePolicy (W1, W2, R1, R2), PrioritySourcePolicy (P1 raw-mat),
///                SplitSourcePolicy (R3 splits W1/W2)
///   Transport  : FastestLanePolicy (P1→warehouses), CheapestLanePolicy (W1→retailers),
///                DirectShipPolicy (default on W2)
///   Production : MakeToStockPolicy (P1 Widget)
/// </summary>
public class MultiEchelonSupplyChainRun
{
    [Fact(Timeout = 120_000)]
    public void MultiEchelonManufacturing_Runs()
        => Assert.NotEqual(3, MultiEchelonModel.Run());
}

internal class MultiEchelonModel : SupplyChainModel
{
    internal MultiEchelonModel()
        : base(null, "Multi-Echelon Manufacturing Model", true, false) { }

    public override void init()
    {
        // ── nodes ────────────────────────────────────────────────────────────
        var s1 = AddNode(new Supplier("S1", "Primary Raw-Mat Supplier"));
        var s2 = AddNode(new Supplier("S2", "Backup Raw-Mat Supplier"));
        var p1 = AddNode(new Plant("P1", "Manufacturing Plant"));
        var w1 = AddNode(new Warehouse("W1", "Main Warehouse"));
        var w2 = AddNode(new Warehouse("W2", "Regional Warehouse"));
        var r1 = AddNode(new Retailer("R1", "Retailer East"));
        var r2 = AddNode(new Retailer("R2", "Retailer West"));
        var r3 = AddNode(new Retailer("R3", "Retailer North"));

        // ── items ─────────────────────────────────────────────────────────────
        var rawMat = AddItem(new Item("RM-01", "Raw Material", unitCost: 5, salesPrice: 5));
        var widget = AddItem(new Item("WG-01", "Widget", unitCost: 15, salesPrice: 40));

        // ── transport lanes ───────────────────────────────────────────────────

        // raw material supply to plant (two sources for PrioritySourcePolicy)
        var ltS1P1 = new ContDistExponential(this, "LT_S1_P1", 2.0, false, false);
        ltS1P1.setNonNegative(true);
        var ltS2P1 = new ContDistExponential(this, "LT_S2_P1", 4.0, false, false);
        ltS2P1.setNonNegative(true);

        AddLane(new TransportLane(s1, p1, rawMat, ltS1P1, capacity: 0,
            costPerUnit: 3.0, meanLeadTime: 2.0));
        AddLane(new TransportLane(s2, p1, rawMat, ltS2P1, capacity: 0,
            costPerUnit: 1.0, meanLeadTime: 4.0));

        // plant to warehouses — P1 uses FastestLanePolicy (prefers W1, mean 1d vs 2d)
        var ltP1W1 = new ContDistExponential(this, "LT_P1_W1", 1.0, false, false);
        ltP1W1.setNonNegative(true);
        var ltP1W2 = new ContDistExponential(this, "LT_P1_W2", 2.0, false, false);
        ltP1W2.setNonNegative(true);

        AddLane(new TransportLane(p1, w1, widget, ltP1W1, capacity: 0,
            costPerUnit: 5.0, meanLeadTime: 1.0));
        AddLane(new TransportLane(p1, w2, widget, ltP1W2, capacity: 0,
            costPerUnit: 2.0, meanLeadTime: 2.0));

        // W1 to retailers — W1 uses CheapestLanePolicy
        var ltW1R1 = new ContDistExponential(this, "LT_W1_R1", 0.5, false, false);
        ltW1R1.setNonNegative(true);
        var ltW1R2 = new ContDistExponential(this, "LT_W1_R2", 0.5, false, false);
        ltW1R2.setNonNegative(true);
        var ltW1R3 = new ContDistExponential(this, "LT_W1_R3", 0.5, false, false);
        ltW1R3.setNonNegative(true);

        AddLane(new TransportLane(w1, r1, widget, ltW1R1, capacity: 0,
            costPerUnit: 1.0, meanLeadTime: 0.5));
        AddLane(new TransportLane(w1, r2, widget, ltW1R2, capacity: 0,
            costPerUnit: 1.0, meanLeadTime: 0.5));
        AddLane(new TransportLane(w1, r3, widget, ltW1R3, capacity: 0,
            costPerUnit: 1.0, meanLeadTime: 0.5));

        // W2 to R3 (second source for R3's SplitSourcePolicy)
        var ltW2R3 = new ContDistExponential(this, "LT_W2_R3", 1.0, false, false);
        ltW2R3.setNonNegative(true);
        AddLane(new TransportLane(w2, r3, widget, ltW2R3, capacity: 0,
            costPerUnit: 0.5, meanLeadTime: 1.0));

        // ── plant policies ────────────────────────────────────────────────────

        // P1 procures raw material: prefer S1, fall back to S2
        AttachPolicy(p1, rawMat, new FixedOrderQuantityPolicy(r: 100, Q: 300));
        AttachPolicy(p1, new PrioritySourcePolicy([s1, s2]));

        // P1 production: make Widget to stock, BOM = 2 units of RawMat per Widget
        var prodLeadTime = new ContDistExponential(this, "LT_Production", 1.0, false, false);
        prodLeadTime.setNonNegative(true);
        var bom = new Dictionary<Item, Dictionary<Item, double>>
        {
            [widget] = new() { [rawMat] = 2.0 }
        };
        AttachPolicy(p1, widget, new MakeToStockPolicy(
            reorderPoint: 50, targetLevel: 300, bom: bom,
            leadTimeDist: prodLeadTime));

        // P1 ships to warehouses using the fastest lane (lower mean lead time wins)
        AttachPolicy(p1, new FastestLanePolicy());

        // ── warehouse policies ────────────────────────────────────────────────

        // W1: base-stock at B=400; sources Widget from P1
        AttachPolicy(w1, widget, new BaseStockPolicy(B: 400));
        AttachPolicy(w1, new SingleSourcePolicy(p1));
        AttachPolicy(w1, new CheapestLanePolicy());       // cheapest lane to retailers

        // W2: min-max; sources Widget from P1
        AttachPolicy(w2, widget, new MinMaxPolicy(s: 50, S: 300));
        AttachPolicy(w2, new SingleSourcePolicy(p1));

        // ── retailer policies ─────────────────────────────────────────────────

        // R1: Poisson demand, min-max replenishment from W1
        AttachPolicy(r1, widget, new MinMaxPolicy(s: 30, S: 150));
        AttachPolicy(r1, new SingleSourcePolicy(w1));
        AttachPolicy(r1, widget,
            new PoissonDemandPolicy(this, arrivalRate: 2.0, meanQty: 8.0,
                name: "Demand_R1"));

        // R2: fixed-interval periodic demand, (r,Q) replenishment from W1
        var r2Qty = new ContDistUniform(this, "R2_OrderQty", 5.0, 20.0, false, false);
        AttachPolicy(r2, widget, new FixedOrderQuantityPolicy(r: 20, Q: 100));
        AttachPolicy(r2, new SingleSourcePolicy(w1));
        AttachPolicy(r2, widget,
            new PeriodicDemandPolicy(this, interval: 1.0, quantityDist: r2Qty,
                name: "Demand_R2"));

        // R3: trace-driven demand, base-stock, splits sourcing 70/30 between W1 and W2
        var r3Trace = new List<(double interArrival, double qty)>
        {
            (0.5, 15), (1.0, 8), (0.3, 20), (2.0, 5), (0.8, 12),
            (1.5, 10), (0.4, 18), (1.2, 7), (0.6, 14), (1.8, 9),
        };
        AttachPolicy(r3, widget, new BaseStockPolicy(B: 80));
        AttachPolicy(r3, new SplitSourcePolicy(new Dictionary<SupplyChainNode, double>
        {
            [w1] = 0.7,
            [w2] = 0.3,
        }));
        AttachPolicy(r3, widget,
            new TraceDrivenDemandPolicy(this, r3Trace, name: "Demand_R3"));

        // ── initial inventory ─────────────────────────────────────────────────
        SetInitialInventory(p1,  rawMat, onHand: 600);
        SetInitialInventory(p1,  widget, onHand: 150);
        SetInitialInventory(w1,  widget, onHand: 350);
        SetInitialInventory(w2,  widget, onHand: 200);
        SetInitialInventory(r1,  widget, onHand: 100);
        SetInitialInventory(r2,  widget, onHand: 60);
        SetInitialInventory(r3,  widget, onHand: 50);

        base.init();
    }

    internal static int Run()
    {
        var model = new MultiEchelonModel();
        var exp   = new Experiment("MultiEchelonExperiment", TimeUnit.DAYS, TimeUnit.DAYS, null);
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
