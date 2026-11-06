// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.supplychain.masterdata;
using ORTools4AI.Simulator.extensions.supplychain.policies.sourcing;
using ORTools4AI.Simulator.Tests.Helpers;

namespace ORTools4AI.Simulator.Tests.extensions.supplychain;

public class SingleSourcePolicyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _consumer;
    private readonly SupplyChainNode _source;
    private readonly Item _item;

    public SingleSourcePolicyTests()
    {
        (_, _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _consumer = new SupplyChainNode("W1", "Warehouse", NodeType.Warehouse);
        _source   = new SupplyChainNode("S1", "Supplier", NodeType.Supplier);
        _item     = new Item("SKU-1", "Widget");
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void GetCandidateSources_SourceAvailable_ReturnsSingleSource()
    {
        var policy = new SingleSourcePolicy(_source);
        var candidates = policy.GetCandidateSources(_consumer, _item, [_source]);
        Assert.Single(candidates);
        Assert.Equal(_source, candidates[0]);
    }

    [Fact]
    public void GetCandidateSources_SourceUnavailable_ReturnsEmpty()
    {
        var other = new SupplyChainNode("S2", "Other", NodeType.Supplier);
        var policy = new SingleSourcePolicy(_source);
        var candidates = policy.GetCandidateSources(_consumer, _item, [other]);
        Assert.Empty(candidates);
    }

    [Fact]
    public void AllocateOrder_AllocatesFullQuantityToSource()
    {
        var policy = new SingleSourcePolicy(_source);
        var alloc = policy.AllocateOrder(_consumer, _item, 100.0, [_source], _now);
        Assert.Equal(100.0, alloc[_source], precision: 6);
    }

    [Fact]
    public void AllocateOrder_ZeroQuantity_AllocatesZero()
    {
        var policy = new SingleSourcePolicy(_source);
        var alloc = policy.AllocateOrder(_consumer, _item, 0.0, [_source], _now);
        Assert.Equal(0.0, alloc[_source], precision: 6);
    }
}

// ---------------------------------------------------------------------------
// PrioritySourcePolicy tests
// ---------------------------------------------------------------------------

public class PrioritySourcePolicyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _consumer;
    private readonly SupplyChainNode _s1;
    private readonly SupplyChainNode _s2;
    private readonly Item _item;

    public PrioritySourcePolicyTests()
    {
        (_, _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _consumer = new SupplyChainNode("W1", "Warehouse", NodeType.Warehouse);
        _s1 = new SupplyChainNode("S1", "Primary", NodeType.Supplier);
        _s2 = new SupplyChainNode("S2", "Backup", NodeType.Supplier);
        _item = new Item("SKU-1", "Widget");
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void GetCandidateSources_BothAvailable_ReturnsBothInOrder()
    {
        var policy = new PrioritySourcePolicy([_s1, _s2]);
        var candidates = policy.GetCandidateSources(_consumer, _item, [_s1, _s2]);
        Assert.Equal(2, candidates.Count);
        Assert.Equal(_s1, candidates[0]);
    }

    [Fact]
    public void GetCandidateSources_PrimaryUnavailable_ReturnsFallback()
    {
        var policy = new PrioritySourcePolicy([_s1, _s2]);
        var candidates = policy.GetCandidateSources(_consumer, _item, [_s2]);
        Assert.Single(candidates);
        Assert.Equal(_s2, candidates[0]);
    }

    [Fact]
    public void GetCandidateSources_NoneAvailable_ReturnsEmpty()
    {
        var s3 = new SupplyChainNode("S3", "Other", NodeType.Supplier);
        var policy = new PrioritySourcePolicy([_s1, _s2]);
        var candidates = policy.GetCandidateSources(_consumer, _item, [s3]);
        Assert.Empty(candidates);
    }

    [Fact]
    public void AllocateOrder_BothAvailable_AllocatesToFirst()
    {
        var policy = new PrioritySourcePolicy([_s1, _s2]);
        var alloc = policy.AllocateOrder(_consumer, _item, 100.0, [_s1, _s2], _now);
        double s1Share = alloc.GetValueOrDefault(_s1, 0.0);
        double total = alloc.Values.Sum();
        Assert.Equal(100.0, total, precision: 6);
        Assert.True(s1Share > 0);
    }
}

// ---------------------------------------------------------------------------
// SplitSourcePolicy tests
// ---------------------------------------------------------------------------

public class SplitSourcePolicyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly TimeInstant _now;
    private readonly SupplyChainNode _consumer;
    private readonly SupplyChainNode _s1;
    private readonly SupplyChainNode _s2;
    private readonly Item _item;

    public SplitSourcePolicyTests()
    {
        (_, _exp) = SimTestHarness.Create();
        _now = new TimeInstant(0L, TimeUnit.SECONDS);
        _consumer = new SupplyChainNode("W1", "Warehouse", NodeType.Warehouse);
        _s1 = new SupplyChainNode("S1", "Source1", NodeType.Supplier);
        _s2 = new SupplyChainNode("S2", "Source2", NodeType.Supplier);
        _item = new Item("SKU-1", "Widget");
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void GetCandidateSources_BothInProportions_ReturnsBoth()
    {
        var policy = new SplitSourcePolicy(new Dictionary<SupplyChainNode, double>
        {
            [_s1] = 0.7, [_s2] = 0.3
        });
        var candidates = policy.GetCandidateSources(_consumer, _item, [_s1, _s2]);
        Assert.Equal(2, candidates.Count);
    }

    [Fact]
    public void GetCandidateSources_OnlyOneAvailable_ReturnsJustThatOne()
    {
        var policy = new SplitSourcePolicy(new Dictionary<SupplyChainNode, double>
        {
            [_s1] = 0.7, [_s2] = 0.3
        });
        var candidates = policy.GetCandidateSources(_consumer, _item, [_s1]);
        Assert.Single(candidates);
        Assert.Equal(_s1, candidates[0]);
    }

    [Fact]
    public void AllocateOrder_SplitsProportion70_30()
    {
        var policy = new SplitSourcePolicy(new Dictionary<SupplyChainNode, double>
        {
            [_s1] = 0.7, [_s2] = 0.3
        });
        var alloc = policy.AllocateOrder(_consumer, _item, 100.0, [_s1, _s2], _now);
        Assert.Equal(70.0, alloc[_s1], precision: 6);
        Assert.Equal(30.0, alloc[_s2], precision: 6);
    }

    [Fact]
    public void AllocateOrder_TotalMatchesSumOfAllocations()
    {
        var policy = new SplitSourcePolicy(new Dictionary<SupplyChainNode, double>
        {
            [_s1] = 0.6, [_s2] = 0.4
        });
        var alloc = policy.AllocateOrder(_consumer, _item, 200.0, [_s1, _s2], _now);
        Assert.Equal(200.0, alloc.Values.Sum(), precision: 6);
    }
}
