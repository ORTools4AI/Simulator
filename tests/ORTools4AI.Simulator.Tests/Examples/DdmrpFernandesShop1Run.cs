// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using ORTools4AI.Simulator.extensions.supplychain.ddmrp;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.process;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using BacklogQueue = System.Collections.Generic.Queue<ORTools4AI.Simulator.core.engine.TimeInstant>;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test;

/// <summary>
/// Replication of Fernandes et al. (2025) Shop Configuration 1:
///
///   RMB (unlimited) → M6 → M5 → M4 → M3* → M2 → M1 → FGB → Customers
///                                         * bottleneck, ~94% utilisation
///
/// 10 product types (A0–A9), exponential demand Exp(68 min) shared equally.
/// Only the Finished Goods Buffer (FGB) carries a DDMRP buffer policy.
/// Station service and setup times from Table 3 (Fernandes 2025).
/// Buffer levels from Table 4/5: GZ=4, RZ=2, YZ=1 → TOY=3, TOG=7.
///
/// The test verifies the simulation runs to completion with a non-negative fill-rate.
/// </summary>
public class DdmrpFernandesShop1Run
{
    [Fact(Timeout = 300_000)]
    public void FernandesShop1_RunsToCompletion()
    {
        double fillRate = FernandesModel.Run(simulationMinutes: 26_000, warmUpMinutes: 6_000);
        Assert.True(fillRate >= 0.0 && fillRate <= 1.0,
            $"Fill rate {fillRate:P1} is out of range [0,1].");
    }
}

// ---------------------------------------------------------------------------
// Model
// ---------------------------------------------------------------------------

internal class FernandesModel : Model
{
    internal const int ProductCount = 10;

    // Station processing and setup means (minutes), index 0=M1 .. 5=M6
    private static readonly double[] ProcessingMeans = [25, 18, 13, 9, 5, 5];
    private static readonly double[] SetupMeans      = [5,  5,  10, 9, 5, 5];

    // products A0..A9
    internal readonly Item[] Products = new Item[ProductCount];

    // FGB state per product
    internal readonly double[] FgbOnHand  = new double[ProductCount];
    internal readonly double[] FgbOnOrder = new double[ProductCount];
    internal readonly BacklogQueue[] Backlog;

    // DDMRP buffers: RZ=2, YZ=1, GZ=4
    internal readonly DdmrpBuffer[] Buffers = new DdmrpBuffer[ProductCount];

    // stations: index 0=M1 (final), 5=M6 (first raw-material stage)
    internal CapacityStationProcess[] Stations = null!;
    private  SupplyChainNode[]        _stationNodes = null!;

    // statistics
    internal Count[] FillCount  = null!;
    internal Count[] TotalCount = null!;
    internal Tally[] LtTally    = null!;

    // demand distributions (populated in init, after connectToExperiment)
    internal ContDistExponential DemandIat      = null!;
    internal ContDistUniform     ProductSelector = null!;

    internal FernandesModel()
        : base(null, "Fernandes-Shop1-DDMRP", true, false)
    {
        Backlog = new BacklogQueue[ProductCount];
        for (int i = 0; i < ProductCount; i++)
            Backlog[i] = new BacklogQueue();
    }

    public override string description() => "Fernandes et al. (2025) Shop Config 1 — DDMRP FGB.";

    public override void init()
    {
        for (int i = 0; i < ProductCount; i++)
            Products[i] = new Item($"A{i}", $"Product A{i}");

        // buffer: TOY=3, TOG=7 (paper GZ=4, RZ=2, YZ=1)
        for (int i = 0; i < ProductCount; i++)
            Buffers[i] = DdmrpBuffer.FromZones(redZone: 2, yellowZone: 1, greenZone: 4);

        FillCount  = new Count[ProductCount];
        TotalCount = new Count[ProductCount];
        LtTally    = new Tally[ProductCount];
        for (int i = 0; i < ProductCount; i++)
        {
            FillCount[i]  = new Count(this, $"Fill-A{i}",  true, false);
            TotalCount[i] = new Count(this, $"Total-A{i}", true, false);
            LtTally[i]    = new Tally(this, $"LT-A{i}",    true, false);
        }

        // station pseudo-nodes (naming only)
        _stationNodes = new SupplyChainNode[6];
        for (int s = 0; s < 6; s++)
            _stationNodes[s] = new SupplyChainNode($"M{s + 1}", $"Station M{s + 1}", NodeType.Plant);

        DemandIat = new ContDistExponential(this, "DemandIAT", 68.0, false, false);
        DemandIat.setNonNegative(true);
        // Uniform(0, 10) — floor gives uniform product selection
        ProductSelector = new ContDistUniform(this, "ProdSel", 0.0, ProductCount, false, false);
    }

    public override void doInitialSchedules()
    {
        Stations = new CapacityStationProcess[6];
        for (int s = 0; s < 6; s++)
        {
            var proc  = new ContDistExponential(this, $"Proc-M{s+1}", ProcessingMeans[s], false, false);
            proc.setNonNegative(true);
            var setup = new ContDistExponential(this, $"Setup-M{s+1}", SetupMeans[s], false, false);
            setup.setNonNegative(true);

            Stations[s] = new CapacityStationProcess(this, _stationNodes[s], proc, setup, false);
            Stations[s].activate(new TimeSpan(0.0));
        }

        // seed FGB at TOG
        for (int i = 0; i < ProductCount; i++)
            FgbOnHand[i] = Buffers[i].TopOfGreen;

        new FernandesDemandProcess(this).activate(new TimeSpan(0.0));
        new FernandesReplenProcess(this).activate(new TimeSpan(0.0));
    }

    // Delivery callback: invoked by M1 station's OnComplete.
    internal void DeliverToFgb(int productIdx, TimeInstant placedAt)
    {
        FgbOnHand[productIdx] += 1.0;
        FgbOnOrder[productIdx] = Math.Max(0, FgbOnOrder[productIdx] - 1.0);

        // drain backlog (one unit per delivery)
        if (Backlog[productIdx].Count > 0)
        {
            var bp = Backlog[productIdx].Dequeue();
            FgbOnHand[productIdx] -= 1.0;
            double blt = presentTime().getTimeAsDouble(TimeUnit.MINUTES)
                       - bp.getTimeAsDouble(TimeUnit.MINUTES);
            LtTally[productIdx].update(blt);
            FillCount[productIdx].update(1);
        }
    }

    // Builds a production job that chains M6→M5→M4→M3→M2→M1→FGB.
    internal ProductionJob CreateChain(int productIdx, TimeInstant placedAt)
    {
        var item = Products[productIdx];
        int p    = productIdx;

        // Innermost: M1 completes → deliver to FGB
        Action<Order> m1Done = _ => DeliverToFgb(p, placedAt);

        // Each outer layer enqueues the next-inner job
        Action<Order> m2Done = _ => Stations[0].Enqueue(
            new ProductionJob(item, 1.0, _stationNodes[1], _stationNodes[0], placedAt, m1Done));
        Action<Order> m3Done = _ => Stations[1].Enqueue(
            new ProductionJob(item, 1.0, _stationNodes[2], _stationNodes[1], placedAt, m2Done));
        Action<Order> m4Done = _ => Stations[2].Enqueue(
            new ProductionJob(item, 1.0, _stationNodes[3], _stationNodes[2], placedAt, m3Done));
        Action<Order> m5Done = _ => Stations[3].Enqueue(
            new ProductionJob(item, 1.0, _stationNodes[4], _stationNodes[3], placedAt, m4Done));
        Action<Order> m6Done = _ => Stations[4].Enqueue(
            new ProductionJob(item, 1.0, _stationNodes[5], _stationNodes[4], placedAt, m5Done));

        // Root job enters M6 (index 5)
        return new ProductionJob(item, 1.0, _stationNodes[5], _stationNodes[4], placedAt, m6Done);
    }

    internal double OverallFillRate()
    {
        long filled = 0, total = 0;
        for (int i = 0; i < ProductCount; i++)
        {
            filled += FillCount[i].getValue();
            total  += TotalCount[i].getValue();
        }
        return total == 0 ? 0.0 : (double)filled / total;
    }

    internal static double Run(double simulationMinutes = 26_000, double warmUpMinutes = 6_000)
    {
        var savedEpsilon = TimeOperations.getEpsilon();
        var savedRef     = TimeOperations.getReferenceUnit();
        try
        {
            var model = new FernandesModel();
            var exp   = new Experiment("FernandesExp", TimeUnit.MINUTES, TimeUnit.MINUTES, null);
            model.connectToExperiment(exp);

            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(simulationMinutes, TimeUnit.MINUTES));

            exp.start();
            exp.report();
            double rate = model.OverallFillRate();
            exp.finish();
            return rate;
        }
        finally
        {
            TimeOperations.setEpsilon(savedEpsilon);
            TimeOperations.setReferenceUnit(savedRef);
        }
    }
}

// ---------------------------------------------------------------------------
// Demand process: Exp(68 min) inter-arrival, uniform product selection, qty=1
// ---------------------------------------------------------------------------

internal class FernandesDemandProcess : SimProcess
{
    private readonly FernandesModel _model;

    internal FernandesDemandProcess(FernandesModel model)
        : base(model, "FernandesDemand", false)
    {
        _model = model;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            yield return Delay(_model.DemandIat.sample());

            int p = Math.Min((int)_model.ProductSelector.sample(), FernandesModel.ProductCount - 1);
            _model.TotalCount[p].update(1);

            if (_model.FgbOnHand[p] >= 1.0)
            {
                _model.FgbOnHand[p] -= 1.0;
                _model.FillCount[p].update(1);
                _model.LtTally[p].update(0); // immediate fill
            }
            else
            {
                _model.Backlog[p].Enqueue(presentTime());
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Replenishment review: quasi-continuous NFP check (every 1 min)
// ---------------------------------------------------------------------------

internal class FernandesReplenProcess : SimProcess
{
    private const double ReviewInterval = 1.0; // minutes
    private readonly FernandesModel _model;

    internal FernandesReplenProcess(FernandesModel model)
        : base(model, "FernandesReplen", false)
    {
        _model = model;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            yield return Delay(ReviewInterval);

            for (int p = 0; p < FernandesModel.ProductCount; p++)
            {
                double nfp = _model.FgbOnHand[p] + _model.FgbOnOrder[p];
                if (nfp >= _model.Buffers[p].TopOfRed) continue;

                int units = (int)Math.Max(1,
                    Math.Round(_model.Buffers[p].TopOfGreen - nfp));
                _model.FgbOnOrder[p] += units;

                for (int u = 0; u < units; u++)
                    _model.Stations[5].Enqueue(_model.CreateChain(p, presentTime()));
            }
        }
    }
}
