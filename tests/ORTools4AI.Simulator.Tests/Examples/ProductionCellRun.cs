// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using ORTools4AI.Simulator.extensions.supplychain.production;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test;

/// <summary>
/// Production cell simulation:
///
///   ConveyorProcess (Exp 45 s IAT, types A/B/C uniform)
///       ↓
///   PartRouter
///       ├── "A" → StationA (load Exp 8 s + drill Exp 12 s)
///       └── "B","C" → BatchGateProcess (min=2, maxWait=90 s)
///                         ↓
///                     ParallelStationGroup
///                         ├── StationBC1 (load Exp 10 s + weld Exp 15 s, setup B↔C)
///                         └── StationBC2 (same, independent distributions)
///   All completions → FiniteBuffer depot (capacity=100)
/// </summary>
public class ProductionCellRun
{
    [Fact(Timeout = 60_000)]
    public void ProductionCell_OneShift_DeliversParts()
    {
        var result = ProductionCellModel.Run(shiftMinutes: 480);
        Assert.True(result.Completed > 0);
    }

    [Fact(Timeout = 60_000)]
    public void ProductionCell_DepotNeverOverflows()
    {
        var result = ProductionCellModel.Run(shiftMinutes: 480);
        Assert.Equal(0, result.Overflows);
    }
}

internal record struct RunResult(int Completed, int Generated, int Overflows);

internal class ProductionCellModel : Model
{
    internal Count         PartsGenerated = null!;
    internal Count         PartsCompleted = null!;
    internal Count         DepotOverflows = null!;
    internal FiniteBuffer  Depot          = null!;

    private MultiPhaseStationProcess _stationA      = null!;
    private MultiPhaseStationProcess _stationBC1    = null!;
    private MultiPhaseStationProcess _stationBC2    = null!;
    private ParallelStationGroup     _parallelGroup  = null!;
    private BatchGateProcess         _batchGate      = null!;
    private ConveyorProcess          _conveyor       = null!;
    private PartRouter               _router         = null!;
    private ContDistUniform          _typeSel        = null!;

    internal ProductionCellModel()
        : base(null, "ProductionCell", true, false) { }

    public override string description() =>
        "Production cell: conveyor → router → station A / batch gate → parallel BC stations → depot.";

    public override void init()
    {
        PartsGenerated = new Count(this, "PartsGenerated", true, false);
        PartsCompleted = new Count(this, "PartsCompleted", true, false);
        DepotOverflows = new Count(this, "DepotOverflows",  true, false);
        Depot          = new FiniteBuffer(100);

        _typeSel = new ContDistUniform(this, "TypeSel", 0.0, 3.0, false, false);
    }

    public override void doInitialSchedules()
    {
        // ── Shared completion handler ─────────────────────────────────────────────
        Action<IReadOnlyList<Part>> CompleteToDepot = parts =>
        {
            foreach (var p in parts)
            {
                if (Depot.TryAdd(p)) PartsCompleted.update(1);
                else                 DepotOverflows.update(1);
            }
        };

        // ── Station A: load (Exp 8 s) + drill (Exp 12 s) ─────────────────────────
        var loadA  = new ContDistExponential(this, "LoadA",  8.0,  false, false);
        var drillA = new ContDistExponential(this, "DrillA", 12.0, false, false);
        loadA.setNonNegative(true);
        drillA.setNonNegative(true);

        _stationA = new MultiPhaseStationProcess(this, "A", [loadA, drillA]);
        _stationA.activate(new TimeSpan(0.0));

        // ── Stations BC1 & BC2: independent distributions, same structure ─────────
        MultiPhaseStationProcess MakeBcStation(string id)
        {
            var load  = new ContDistExponential(this, $"Load{id}",    10.0, false, false);
            var weld  = new ContDistExponential(this, $"Weld{id}",    15.0, false, false);
            var setupBC = new ContDistExponential(this, $"SetupBC{id}", 8.0, false, false);
            var setupCB = new ContDistExponential(this, $"SetupCB{id}", 12.0, false, false);
            load.setNonNegative(true);    weld.setNonNegative(true);
            setupBC.setNonNegative(true); setupCB.setNonNegative(true);

            return new MultiPhaseStationProcess(this, id, [load, weld],
                setupMatrix: new Dictionary<(string, string), NumericalDist<double>>
                {
                    [("B", "C")] = setupBC,
                    [("C", "B")] = setupCB,
                });
        }

        _stationBC1 = MakeBcStation("BC1");
        _stationBC2 = MakeBcStation("BC2");
        _stationBC1.activate(new TimeSpan(0.0));
        _stationBC2.activate(new TimeSpan(0.0));

        _parallelGroup = new ParallelStationGroup([_stationBC1, _stationBC2]);

        // ── Batch gate: min=2, max wait=90 s → parallel group ────────────────────
        _batchGate = new BatchGateProcess(
            this, "BC",
            minBatchSize: 2,
            maxWaitMs:    90.0,   // seconds (reference unit is TimeUnit.SECONDS)
            downstream:   job => _parallelGroup.Enqueue(job with { OnComplete = CompleteToDepot }));
        _batchGate.activate(new TimeSpan(0.0));

        // ── Router ────────────────────────────────────────────────────────────────
        _router = new PartRouter(
            classify: p => p.PartType,
            routes: new Dictionary<string, Action<Part>>
            {
                ["A"] = p => _stationA.Enqueue(
                    new StationJob([p], "A", presentTime(), CompleteToDepot)),
                ["B"] = p => _batchGate.Arrive(p),
                ["C"] = p => _batchGate.Arrive(p),
            });

        // ── Conveyor: Exp(45 s) IAT, uniform type selection ───────────────────────
        var iat = new ContDistExponential(this, "IAT", 45.0, false, false);
        iat.setNonNegative(true);

        _conveyor = new ConveyorProcess(
            this, "Main",
            iatDist:          iat,
            partTypeSelector: () =>
            {
                int idx = Math.Min((int)_typeSel.sample(), 2);
                return idx switch { 0 => "A", 1 => "B", _ => "C" };
            },
            onArrival: p =>
            {
                PartsGenerated.update(1);
                _router.Route(p);
            });
        _conveyor.activate(new TimeSpan(0.0));
    }

    internal static RunResult Run(double shiftMinutes = 480)
    {
        var savedEpsilon = TimeOperations.getEpsilon();
        var savedRef     = TimeOperations.getReferenceUnit();
        try
        {
            var model = new ProductionCellModel();
            var exp   = new Experiment("ProductionCellExp", TimeUnit.SECONDS, TimeUnit.SECONDS, null);
            model.connectToExperiment(exp);

            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(shiftMinutes * 60.0, TimeUnit.SECONDS));

            exp.start();
            exp.report();
            var result = new RunResult(
                Completed:  (int)model.PartsCompleted.getValue(),
                Generated:  (int)model.PartsGenerated.getValue(),
                Overflows:  (int)model.DepotOverflows.getValue());
            exp.finish();
            return result;
        }
        finally
        {
            TimeOperations.setEpsilon(savedEpsilon);
            TimeOperations.setReferenceUnit(savedRef);
        }
    }
}
