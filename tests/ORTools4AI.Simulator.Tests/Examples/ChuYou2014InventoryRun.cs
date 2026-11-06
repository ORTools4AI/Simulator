// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.ai;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test;

// ─────────────────────────────────────────────────────────────────────────────
// Paper: Chu & You (2014) "Simulation-Based Optimization for Multi-Echelon
//        Inventory Systems Under Uncertainty", Winter Simulation Conference.
//
// Network (divergent / tree):
//   Plant (unlimited) → DC1 → Distributor2
//                           → Distributor3
//                           → Distributor4
//
// Policy:  (r, Q) at DC1 and each distributor (FixedOrderQuantity).
// Demand:  Normal distribution at each customer-facing distributor (daily).
// Lead times: Uniform on each arc.
// Metric:  fill rate (β service level) ≥ 95%, minimise holding + reorder cost.
//
// This test demonstrates the Desmoj AI framework:
//   • IDecisionAgent<ChuYouObs, double[]>  — (r,Q) policy as a swappable agent
//   • AiSimProcess                         — daily review driven by the agent
//   • IRewardFunction                      — negative incremental cost per day
//   • MultiEpisodeRunner                   — compares InitialPolicy vs OptimalPolicy
// ─────────────────────────────────────────────────────────────────────────────

public class ChuYou2014InventoryRun
{
    // Baseline: initial (conservative) parameters from Table 1 of the paper.
    [Fact(Timeout = 60_000)]
    public void InitialPolicy_Runs365Days()
    {
        var result = RunSingleEpisode(RqAgent.InitialPolicy(), seed: 42);
        Assert.All(result.FillRates, fr => Assert.True(fr >= 0.85,
            $"fill rate {fr:P1} should be ≥ 85% even for conservative parameters"));
    }

    // Optimal: paper optimal parameters should clear the 90% hurdle reliably.
    [Fact(Timeout = 60_000)]
    public void OptimalPolicy_AchievesHighFillRate()
    {
        var result = RunSingleEpisode(RqAgent.OptimalPolicy(), seed: 42);
        Assert.All(result.FillRates, fr => Assert.True(fr >= 0.90,
            $"fill rate {fr:P1} should be ≥ 90% (paper target is 95%)"));
    }

    // Key paper result: optimised parameters reduce inventory cost.
    [Fact(Timeout = 120_000)]
    public void OptimalPolicy_CostsLessThanInitialPolicy()
    {
        double costInitial = AverageEpisodeCost(RqAgent.InitialPolicy(), episodes: 3, baseSeed: 1);
        double costOptimal = AverageEpisodeCost(RqAgent.OptimalPolicy(), episodes: 3, baseSeed: 1);

        Assert.True(costOptimal < costInitial,
            $"Optimal mean cost {costOptimal:F0} should be lower than initial {costInitial:F0}");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static ChuYou2014Model RunSingleEpisode(RqAgent agent, long seed)
    {
        var savedEps = TimeOperations.getEpsilon();
        var savedRef = TimeOperations.getReferenceUnit();
        try
        {
            var model = new ChuYou2014Model(agent);
            model.Run(365, TimeUnit.DAYS, seed);
            return model;
        }
        finally
        {
            TimeOperations.setEpsilon(savedEps);
            TimeOperations.setReferenceUnit(savedRef);
        }
    }

    private static double AverageEpisodeCost(RqAgent agent, int episodes, long baseSeed)
    {
        var rng = new Random((int)baseSeed);
        double sum = 0;
        for (int i = 0; i < episodes; i++)
        {
            var model = RunSingleEpisode(agent, rng.NextInt64());
            sum += model.TotalInventoryCost;
        }
        return sum / episodes;
    }
}

// ── Observation ───────────────────────────────────────────────────────────────
// 12-float state vector: [onHand, onOrder, backlog] × 4 facilities
// Facilities: 0 = DC1, 1 = Distributor2, 2 = Distributor3, 3 = Distributor4

internal sealed class ChuYouObs : VectorObservation
{
    internal ChuYouObs(double[] onHand, double[] onOrder, double[] backlog)
        : base(Build(onHand, onOrder, backlog)) { }

    private static float[] Build(double[] h, double[] o, double[] b)
    {
        var f = new float[12];
        for (int i = 0; i < 4; i++)
        {
            f[i * 3]     = (float)h[i];
            f[i * 3 + 1] = (float)o[i];
            f[i * 3 + 2] = (float)b[i];
        }
        return f;
    }
}

// ── (r, Q) agent ─────────────────────────────────────────────────────────────
// Each call to Decide() returns four order quantities, one per facility.
// Order quantity for facility i = Q_i if inventory position ≤ r_i, else 0.

internal sealed class RqAgent : IDecisionAgent<ChuYouObs, double[]>
{
    private readonly double[] _r;
    private readonly double[] _q;

    internal RqAgent(double[] r, double[] q) { _r = r; _q = q; }

    public double[] Decide(ChuYouObs obs)
    {
        var orders = new double[4];
        for (int i = 0; i < 4; i++)
        {
            // inventory position = onHand + onOrder - backlog
            double ip = obs.Features[i * 3] + obs.Features[i * 3 + 1] - obs.Features[i * 3 + 2];
            orders[i] = ip <= _r[i] ? _q[i] : 0;
        }
        return orders;
    }

    // Paper Table 1 — initial solution (scaled to daily demand of ~100-120 u/distributor).
    internal static RqAgent InitialPolicy() => new(
        r: [1600, 520, 420, 630],
        q: [1500, 500, 400, 600]);

    // Paper Table 1 — optimised solution (lean but maintains service level).
    internal static RqAgent OptimalPolicy() => new(
        r: [1100, 350, 280, 420],
        q: [1500, 500, 400, 600]);
}

// ── Reward function ───────────────────────────────────────────────────────────
// Reward = negative incremental cost since the last decision step.

internal sealed class InventoryCostReward(ChuYou2014Model model) : IRewardFunction
{
    private double _baseline;

    public double Compute()
    {
        double current = model.TotalInventoryCost;
        double delta   = current - _baseline;
        _baseline      = current;
        return -delta;
    }

    public double ComputeTerminal() => 0;
    public void   Reset()           => _baseline = 0;
}

// ── AI-driven daily ordering process ─────────────────────────────────────────
// Subclasses AiSimProcess to wire the (r,Q) agent into the DESMOJ lifecycle.

internal sealed class DailyOrderingProcess(
    ChuYou2014Model model,
    IDecisionAgent<ChuYouObs, double[]> agent)
    : AiSimProcess<ChuYouObs, double[]>(model, agent, new InventoryCostReward(model))
{
    protected override double DecisionInterval => 1.0; // daily review

    protected override ChuYouObs BuildObservation()
    {
        // Process pipeline arrivals and realise customer demand before deciding.
        model.ProcessDeliveries();
        model.AccumulateHoldingCost();
        model.RealizeDemand();
        return new ChuYouObs(model.OnHand, model.OnOrder, model.Backlog);
    }

    protected override void ApplyAction(double[] orders)
    {
        // Step 1: distributor orders → fulfilled from DC1 stock if available.
        for (int i = 1; i <= 3; i++)
            if (orders[i] > 0)
                model.OrderFromDC1(distributorIndex: i, qty: orders[i]);

        // Step 2: DC1 replenishes from Plant (unlimited, always ships after lead time).
        if (orders[0] > 0)
            model.OrderFromPlant(qty: orders[0]);
    }
}

// ── Two-echelon inventory model ───────────────────────────────────────────────

/// <summary>
/// Divergent two-echelon inventory network from Chu &amp; You (2014).
/// Facility indices: 0 = DC1, 1 = Distributor2, 2 = Distributor3, 3 = Distributor4.
/// </summary>
internal sealed class ChuYou2014Model : Model
{
    // ── inventory state ───────────────────────────────────────────────────────
    internal double[] OnHand  = new double[4];
    internal double[] OnOrder = new double[4];
    internal double[] Backlog = new double[4];

    // ── performance counters ──────────────────────────────────────────────────
    private int[]    _filled     = new int[4];
    private int[]    _total      = new int[4];
    private double[] _holdingAcc = new double[4];
    private int[]    _orderCount = new int[4];

    // ── distributions ─────────────────────────────────────────────────────────
    private ContDistNormal?[]  _demandDist   = new ContDistNormal[4];
    private ContDistUniform?[] _leadTimeDist = new ContDistUniform[4];

    // ── order pipeline and DC1 backlog queue ──────────────────────────────────
    private readonly List<(double arrivalDay, int facility, double qty)> _pipeline = [];
    private readonly System.Collections.Generic.Queue<(int distributorIndex, double qty)> _dc1Backlog = [];

    // ── cost parameters ───────────────────────────────────────────────────────
    private const double HoldingCostPerUnitPerDay = 0.50;
    private const double ReorderCostPerOrder      = 50.0;

    private readonly IDecisionAgent<ChuYouObs, double[]> _agent;

    // ── public metrics ────────────────────────────────────────────────────────
    // FillRates[0] = DC1 distributor fill rate, [1-3] = distributor customer fill rate
    internal double[] FillRates =>
        Enumerable.Range(0, 4)
            .Select(i => _total[i] == 0 ? 1.0 : (double)_filled[i] / _total[i])
            .ToArray();

    internal double TotalInventoryCost =>
        _holdingAcc.Sum() + _orderCount.Sum() * ReorderCostPerOrder;

    internal ChuYou2014Model(IDecisionAgent<ChuYouObs, double[]> agent)
        : base(null, "ChuYou2014", false, false) => _agent = agent;

    public override void init()
    {
        // Customer demand at leaf distributors (Normal, truncated at 0).
        _demandDist[1] = new ContDistNormal(this, "Demand_D2", 100, 30, false, false);
        _demandDist[2] = new ContDistNormal(this, "Demand_D3",  80, 25, false, false);
        _demandDist[3] = new ContDistNormal(this, "Demand_D4", 120, 35, false, false);
        _demandDist[1]!.setNonNegative(true);
        _demandDist[2]!.setNonNegative(true);
        _demandDist[3]!.setNonNegative(true);

        // Lead times: Plant→DC1 and DC1→each distributor (Uniform).
        _leadTimeDist[0] = new ContDistUniform(this, "LT_Plant_DC1", 2, 4, false, false);
        _leadTimeDist[1] = new ContDistUniform(this, "LT_DC1_D2",    1, 3, false, false);
        _leadTimeDist[2] = new ContDistUniform(this, "LT_DC1_D3",    1, 3, false, false);
        _leadTimeDist[3] = new ContDistUniform(this, "LT_DC1_D4",    1, 3, false, false);

        // Seed inventory at initial reorder points to start in a stable state.
        OnHand[0] = 1600; // DC1
        OnHand[1] =  520; // Distributor2
        OnHand[2] =  420; // Distributor3
        OnHand[3] =  630; // Distributor4
    }

    public override void doInitialSchedules()
    {
        var proc = new DailyOrderingProcess(this, _agent);
        proc.activate(new TimeSpan(0.0));
    }

    public override string description() =>
        "Chu & You (2014) two-echelon inventory model with AI-driven (r,Q) ordering.";

    // ── simulation operations ─────────────────────────────────────────────────

    internal void ProcessDeliveries()
    {
        double now = presentTime().getTimeAsDouble(TimeUnit.DAYS);

        for (int i = _pipeline.Count - 1; i >= 0; i--)
        {
            var (arrivalDay, facility, qty) = _pipeline[i];
            if (arrivalDay > now + 1e-9) continue;

            _pipeline.RemoveAt(i);
            OnOrder[facility] = Math.Max(0, OnOrder[facility] - qty);

            if (facility == 0)
            {
                // DC1 received goods from Plant — fill DC1 backlog first.
                FillDC1Backlog(qty);
            }
            else
            {
                // Distributor received goods from DC1 — fill customer backlog.
                OnHand[facility] += qty;
                double fillBl = Math.Min(OnHand[facility], Backlog[facility]);
                OnHand[facility]  -= fillBl;
                Backlog[facility] -= fillBl;
            }
        }
    }

    internal void AccumulateHoldingCost()
    {
        for (int i = 0; i < 4; i++)
            _holdingAcc[i] += OnHand[i] * HoldingCostPerUnitPerDay;
    }

    internal void RealizeDemand()
    {
        for (int i = 1; i <= 3; i++)
        {
            double demand = _demandDist[i]!.sample();
            double fill   = Math.Min(OnHand[i], demand);
            OnHand[i]    -= fill;
            Backlog[i]   += demand - fill;
            _filled[i]   += (int)Math.Round(fill);
            _total[i]    += (int)Math.Round(demand);
        }
    }

    // Distributor i orders qty from DC1.
    internal void OrderFromDC1(int distributorIndex, double qty)
    {
        double shipped = Math.Min(OnHand[0], qty);
        double backordered = qty - shipped;

        if (shipped > 0)
        {
            OnHand[0] -= shipped;
            ScheduleDelivery(distributorIndex, shipped);
        }

        if (backordered > 0)
        {
            Backlog[0] += backordered;
            _dc1Backlog.Enqueue((distributorIndex, backordered));
        }

        // Track DC1 fill rate for the distributor-facing service level.
        _filled[0] += (int)Math.Round(shipped);
        _total[0]  += (int)Math.Round(qty);

        _orderCount[distributorIndex]++;
    }

    // DC1 orders qty from Plant (always fulfilled, unlimited supply).
    internal void OrderFromPlant(double qty)
    {
        OnOrder[0] += qty;
        _orderCount[0]++;
        double lt  = _leadTimeDist[0]!.sample();
        double now = presentTime().getTimeAsDouble(TimeUnit.DAYS);
        _pipeline.Add((now + lt, 0, qty));
    }

    private void ScheduleDelivery(int distributorIndex, double qty)
    {
        OnOrder[distributorIndex] += qty;
        double lt  = _leadTimeDist[distributorIndex]!.sample();
        double now = presentTime().getTimeAsDouble(TimeUnit.DAYS);
        _pipeline.Add((now + lt, distributorIndex, qty));
    }

    private void FillDC1Backlog(double arrived)
    {
        // Fill queued distributor backorders before adding remainder to DC1 stock.
        while (arrived > 0 && _dc1Backlog.Count > 0)
        {
            var (distIdx, remaining) = _dc1Backlog.Dequeue();
            double fill    = Math.Min(arrived, remaining);
            arrived       -= fill;
            remaining     -= fill;

            ScheduleDelivery(distIdx, fill);
            Backlog[0] -= fill;

            if (remaining > 0)
                _dc1Backlog.Enqueue((distIdx, remaining));
        }
        OnHand[0] += arrived;
    }
}
