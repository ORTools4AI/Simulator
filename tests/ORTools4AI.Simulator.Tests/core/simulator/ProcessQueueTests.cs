// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.Tests.Helpers;

namespace ORTools4AI.Simulator.Tests.core.simulator;

// Minimal Entity for queue tests
internal sealed class QueueEntity(Model owner, string name)
    : Entity(owner, name, false);

// ---------------------------------------------------------------------------
// ProcessQueue (FIFO) tests
// ---------------------------------------------------------------------------

public class ProcessQueueFifoTests : IDisposable
{
    private readonly HarnessModel _model;
    private readonly Experiment _exp;
    private readonly ProcessQueue<QueueEntity> _queue;

    public ProcessQueueFifoTests()
    {
        (_model, _exp) = SimTestHarness.Create();
        _queue = new ProcessQueue<QueueEntity>(_model, "FifoQ", false, false);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void EmptyQueue_SizeIsZero()
        => Assert.Equal(0, _queue.size());

    [Fact]
    public void EmptyQueue_FirstReturnsNull()
        => Assert.Null(_queue.first());

    [Fact]
    public void Insert_SingleEntity_SizeBecomesOne()
    {
        var e = new QueueEntity(_model, "E1");
        _queue.insert(e);
        Assert.Equal(1, _queue.size());
    }

    [Fact]
    public void Insert_SingleEntity_ReturnsTrue()
    {
        var e = new QueueEntity(_model, "E1");
        Assert.True(_queue.insert(e));
    }

    [Fact]
    public void Insert_Null_ReturnsFalse()
        => Assert.False(_queue.insert(null!));

    [Fact]
    public void Insert_Null_SizeStaysZero()
    {
        _queue.insert(null!);
        Assert.Equal(0, _queue.size());
    }

    [Fact]
    public void Contains_AfterInsert_ReturnsTrue()
    {
        var e = new QueueEntity(_model, "E1");
        _queue.insert(e);
        Assert.True(_queue.contains(e));
    }

    [Fact]
    public void Contains_NotInserted_ReturnsFalse()
    {
        var e = new QueueEntity(_model, "E1");
        Assert.False(_queue.contains(e));
    }

    [Fact]
    public void FIFO_First_ReturnsFirstInserted()
    {
        var e1 = new QueueEntity(_model, "E1");
        var e2 = new QueueEntity(_model, "E2");
        _queue.insert(e1);
        _queue.insert(e2);
        Assert.Equal(e1, _queue.first());
    }

    [Fact]
    public void RemoveFirst_ReturnsFirstEntity()
    {
        var e1 = new QueueEntity(_model, "E1");
        var e2 = new QueueEntity(_model, "E2");
        _queue.insert(e1);
        _queue.insert(e2);
        var removed = _queue.removeFirst();
        Assert.Equal(e1, removed);
    }

    [Fact]
    public void RemoveFirst_DecrementsSize()
    {
        var e1 = new QueueEntity(_model, "E1");
        var e2 = new QueueEntity(_model, "E2");
        _queue.insert(e1);
        _queue.insert(e2);
        _queue.removeFirst();
        Assert.Equal(1, _queue.size());
    }

    [Fact]
    public void Remove_SpecificEntity_RemovesIt()
    {
        var e1 = new QueueEntity(_model, "E1");
        var e2 = new QueueEntity(_model, "E2");
        _queue.insert(e1);
        _queue.insert(e2);
        _queue.remove(e1);
        Assert.False(_queue.contains(e1));
        Assert.True(_queue.contains(e2));
    }

    [Fact]
    public void GetRefused_InitiallyZero()
        => Assert.Equal(0L, _queue.getRefused());

    [Fact]
    public void MultipleInserts_SizeTracksCorrectly()
    {
        for (int i = 0; i < 5; i++)
            _queue.insert(new QueueEntity(_model, $"E{i}"));
        Assert.Equal(5, _queue.size());
    }
}

// Note: LIFO ProcessQueue tests omitted — QueueListLifo.insertBefore is a known
// NotImplementedException stub in the current port. FIFO and capacity tests cover
// the shared ProcessQueue infrastructure.

// ---------------------------------------------------------------------------
// ProcessQueue with capacity limit
// ---------------------------------------------------------------------------

public class ProcessQueueCapacityTests : IDisposable
{
    private readonly HarnessModel _model;
    private readonly Experiment _exp;

    public ProcessQueueCapacityTests()
    {
        (_model, _exp) = SimTestHarness.Create();
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Insert_AtCapacityLimit_ReturnsFalse()
    {
        var queue = new ProcessQueue<QueueEntity>(_model, "CapQ",
            QueueBased.FIFO, 2, false, false);
        queue.insert(new QueueEntity(_model, "E1"));
        queue.insert(new QueueEntity(_model, "E2"));
        bool result = queue.insert(new QueueEntity(_model, "E3"));
        Assert.False(result);
    }

    [Fact]
    public void Insert_AtCapacityLimit_IncrementsRefused()
    {
        var queue = new ProcessQueue<QueueEntity>(_model, "CapQ2",
            QueueBased.FIFO, 2, false, false);
        queue.insert(new QueueEntity(_model, "E1"));
        queue.insert(new QueueEntity(_model, "E2"));
        queue.insert(new QueueEntity(_model, "E3"));
        Assert.Equal(1L, queue.getRefused());
    }

    [Fact]
    public void Insert_ZeroCapacity_TreatedAsUnlimited()
    {
        var queue = new ProcessQueue<QueueEntity>(_model, "UnlimQ",
            QueueBased.FIFO, 0, false, false);
        for (int i = 0; i < 100; i++)
            queue.insert(new QueueEntity(_model, $"E{i}"));
        Assert.Equal(100, queue.size());
    }
}
