// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;
using ORTools4AI.Simulator.extensions.supplychain.demand;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.inventory;
using ORTools4AI.Simulator.extensions.supplychain.policies.production;
using ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;
using ORTools4AI.Simulator.extensions.supplychain.policies.transport;
using ORTools4AI.Simulator.extensions.supplychain.process;
using ORTools4AI.Simulator.extensions.supplychain.transactionaldata;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain;

internal class SupplyChainModel : Model
{
    private readonly List<SupplyChainNode> _nodes = [];
    private readonly List<Item> _items = [];
    private readonly List<TransportLane> _lanes = [];

    // process references built in doInitialSchedules
    private readonly Dictionary<SupplyChainNode, FulfillProcess> _fulfillProcesses = [];
    private readonly Dictionary<TransportLane, TransportProcess> _laneProcesses = [];

    // statistics
    private Dictionary<(SupplyChainNode, Item), Accumulate> _inventoryAccum = [];
    private Dictionary<(SupplyChainNode, Item), Accumulate> _backlogAccum = [];
    private Dictionary<(SupplyChainNode, Item), Count> _fillCount = [];
    private Dictionary<(SupplyChainNode, Item), Count> _totalCount = [];
    private Dictionary<(SupplyChainNode, Item), Count> _lostCount = [];
    private Dictionary<(SupplyChainNode, Item), Tally> _leadTimeTally = [];
    private Dictionary<TransportLane, Count> _throughputCount = [];

    // configurable review and production intervals (default: 1 time unit)
    internal double ReplenishmentReviewInterval { get; set; } = 1.0;
    internal double ProductionReviewInterval { get; set; } = 1.0;

    internal SupplyChainModel(Model? owner, string name, bool showInReport, bool showInTrace)
        : base(owner, name, showInReport, showInTrace) { }

    // ── builder API ──────────────────────────────────────────────────────────

    internal T AddNode<T>(T node) where T : SupplyChainNode
    {
        _nodes.Add(node);
        return node;
    }

    internal Item AddItem(Item item)
    {
        _items.Add(item);
        return item;
    }

    internal TransportLane AddLane(TransportLane lane)
    {
        _lanes.Add(lane);
        return lane;
    }

    internal void AttachPolicy(SupplyChainNode node, Item item, IInventoryPolicy policy)
        => node.InventoryPolicies[item] = policy;

    internal void AttachPolicy(SupplyChainNode node, Item item, IManufacturingPolicy policy)
        => node.ManufacturingPolicies[item] = policy;

    internal void AttachPolicy(SupplyChainNode node, Item item, IDemandPolicy policy)
        => node.DemandPolicies[item] = policy;

    internal void AttachPolicy(SupplyChainNode node, ITransportPolicy policy)
        => node.TransportPolicy = policy;

    internal void AttachPolicy(SupplyChainNode node, ISourcingPolicy policy)
        => node.SourcingPolicy = policy;

    internal void SetInitialInventory(SupplyChainNode node, Item item,
        double onHand, double onReceipt = 0)
    {
        node.Inventory.AdjustOnHand(item, onHand);
        if (onReceipt > 0) node.Inventory.AdjustOnReceipt(item, onReceipt);
    }

    // ── DESMOJ lifecycle ─────────────────────────────────────────────────────

    public override string description()
        => "Supply chain simulation model.";

    public override void init()
    {
        // create statistics for every (node, item) combination that is managed
        foreach (var node in _nodes)
        {
            var managedItems = node.InventoryPolicies.Keys
                .Concat(node.DemandPolicies.Keys)
                .Concat(node.ManufacturingPolicies.Keys)
                .Distinct();

            foreach (var item in managedItems)
            {
                var key = (node, item);
                string tag = $"{node.NodeId}-{item.ItemId}";

                _inventoryAccum[key] = new Accumulate(this, $"Inventory-{tag}", true, false);
                _backlogAccum[key]   = new Accumulate(this, $"Backlog-{tag}", true, false);
                _fillCount[key]      = new Count(this, $"FillCount-{tag}", true, false);
                _totalCount[key]     = new Count(this, $"TotalCount-{tag}", true, false);
                _lostCount[key]      = new Count(this, $"LostCount-{tag}", true, false);
                _leadTimeTally[key]  = new Tally(this, $"LeadTime-{tag}", true, false);
            }
        }

        foreach (var lane in _lanes)
            _throughputCount[lane] = new Count(this,
                $"Throughput-{lane.From.NodeId}-{lane.To.NodeId}-{lane.Item.ItemId}",
                true, false);
    }

    public override void doInitialSchedules()
    {
        // 1. create one FulfillProcess per inventory-holding node
        foreach (var node in _nodes)
        {
            if (node.InventoryPolicies.Count > 0 || node.DemandPolicies.Count > 0)
            {
                var fp = new FulfillProcess(this, node, false);
                _fulfillProcesses[node] = fp;

                // wire statistics into FulfillProcess
                foreach (var item in node.DemandPolicies.Keys
                    .Concat(node.InventoryPolicies.Keys).Distinct())
                {
                    var key = (node, item);
                    if (_fillCount.TryGetValue(key, out var fc))    fp.FillCount[item] = fc;
                    if (_totalCount.TryGetValue(key, out var tc))   fp.TotalCount[item] = tc;
                    if (_lostCount.TryGetValue(key, out var lc))    fp.LostCount[item] = lc;
                    if (_leadTimeTally.TryGetValue(key, out var lt)) fp.LeadTimeTally[item] = lt;
                    if (_backlogAccum.TryGetValue(key, out var ba))  fp.BacklogAccum[item] = ba;
                }

                fp.activate(new TimeSpan(0.0));
            }
        }

        // 2. create one TransportProcess per lane
        foreach (var lane in _lanes)
        {
            var destFp = _fulfillProcesses.GetValueOrDefault(lane.To);
            if (destFp is null) continue;

            var tp = new TransportProcess(this, lane, destFp, false);
            tp.ThroughputCount = _throughputCount.GetValueOrDefault(lane);
            _laneProcesses[lane] = tp;
            tp.activate(new TimeSpan(0.0));
        }

        // 3. create one DemandProcess per (node, item) with a DemandPolicy
        foreach (var node in _nodes)
        {
            if (!_fulfillProcesses.TryGetValue(node, out var fp)) continue;
            foreach (var (item, policy) in node.DemandPolicies)
            {
                var dp = new DemandProcess(this, node, item, policy, fp, false);
                dp.activate(new TimeSpan(0.0));
            }
        }

        // 4. create one ReplenishmentProcess per node with InventoryPolicies + SourcingPolicy
        foreach (var node in _nodes)
        {
            if (node.InventoryPolicies.Count == 0 || node.SourcingPolicy is null) continue;
            var upstream = _nodes
                .Where(n => _lanes.Any(l => l.To == node && l.From == n))
                .ToList();
            var rp = new ReplenishmentProcess(this, node,
                ReplenishmentReviewInterval, upstream, _laneProcesses, false);
            rp.activate(new TimeSpan(0.0));
        }

        // 5. create one ProductionProcess per Plant
        foreach (var node in _nodes)
        {
            if (node is not Plant plant) continue;
            if (plant.ManufacturingPolicies.Count == 0) continue;
            if (!_fulfillProcesses.TryGetValue(plant, out var fp)) continue;
            var pp = new ProductionProcess(this, plant,
                ProductionReviewInterval, fp, false);
            pp.activate(new TimeSpan(0.0));
        }
    }
}
