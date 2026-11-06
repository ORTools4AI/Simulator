// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.Tests.Helpers;

namespace ORTools4AI.Simulator.Tests.core.simulator;

internal class TestEntity : Entity
{
    public TestEntity(Model owner, string name, bool showInTrace = false) : base(owner, name, showInTrace) { }
}

internal class TestEvent : Event
{
    public TestEvent(Model owner) : base(owner, "TestEvent", false) { }
    public override void eventRoutine(Entity who) { }
}

// ── EventNoteTests ─────────────────────────────────────────────────────────

public class EventNoteTests : IDisposable
{
    private readonly Experiment _experiment;
    private readonly HarnessModel _model;

    public EventNoteTests()
    {
        (_model, _experiment) = SimTestHarness.Create();
    }

    public void Dispose() => _experiment.finish();

    private TimeInstant T(long seconds) => new TimeInstant(seconds, TimeUnit.SECONDS);

    [Fact]
    public void Constructor_NullEventAndOneEntity_EntityStoredCorrectly()
    {
        var e1 = new TestEntity(_model,"E1", false);
        var note = new EventNote(e1, null, null, null, T(5), 0, null);

        Assert.Same(e1, note.getEntity1());
        Assert.Null(note.getEntity2());
        Assert.Null(note.getEntity3());
    }

    [Fact]
    public void Constructor_NullEntity2And3_GetNumberOfEntities_IsOne()
    {
        var e1 = new TestEntity(_model,"E1", false);
        var note = new EventNote(e1, null, null, null, T(5), 0, null);
        Assert.Equal(1L, note.getNumberOfEntities());
    }

    [Fact]
    public void Constructor_AllThreeEntities_GetNumberOfEntities_IsThree()
    {
        var e1 = new TestEntity(_model,"E1", false);
        var e2 = new TestEntity(_model,"E2", false);
        var e3 = new TestEntity(_model,"E3", false);
        var note = new EventNote(e1, e2, e3, null, T(5), 0, null);
        Assert.Equal(3L, note.getNumberOfEntities());
    }

    [Fact]
    public void Constructor_NullEntities_GetNumberOfEntities_IsZero()
    {
        var note = new EventNote(null, null, null, null, T(5), 0, null);
        Assert.Equal(0L, note.getNumberOfEntities());
    }

    [Fact]
    public void Constructor_NullEvent_DoesNotThrow()
    {
        var ex = Record.Exception(() => new EventNote(null, null, null, null, T(1), 0, null));
        Assert.Null(ex);
    }

    [Fact]
    public void GetTime_ReturnsTimePassedToConstructor()
    {
        var t = T(42);
        var note = new EventNote(null, null, null, null, t, 0, null);
        Assert.Equal(t.getTimeInEpsilon(), note.getTime().getTimeInEpsilon());
    }

    [Fact]
    public void GetPriority_ReturnsHowImportant()
    {
        var note = new EventNote(null, null, null, null, T(1), 7, null);
        Assert.Equal(7, note.getPriority());
    }

    [Fact]
    public void GetSource_ReturnsSourceSchedulable()
    {
        var src = new TestEntity(_model,"Src", false);
        var note = new EventNote(null, null, null, null, T(1), 0, src);
        Assert.Same(src, note.getSource());
    }

    [Fact]
    public void IsConnected_DefaultFalse()
    {
        var note = new EventNote(null, null, null, null, T(1), 0, null);
        Assert.False(note.isConnected());
    }

    [Fact]
    public void SetConnected_SetsFlag()
    {
        var note = new EventNote(null, null, null, null, T(1), 0, null);
        note.setConnected(true);
        Assert.True(note.isConnected());
        note.setConnected(false);
        Assert.False(note.isConnected());
    }

    [Fact]
    public void CompareTo_EarlierTimeNote_ReturnsNegative()
    {
        var earlier = new EventNote(null, null, null, null, T(5), 0, null);
        var later   = new EventNote(null, null, null, null, T(10), 0, null);
        Assert.True(earlier.CompareTo(later) < 0);
    }

    [Fact]
    public void CompareTo_LaterTimeNote_ReturnsPositive()
    {
        var earlier = new EventNote(null, null, null, null, T(5), 0, null);
        var later   = new EventNote(null, null, null, null, T(10), 0, null);
        Assert.True(later.CompareTo(earlier) > 0);
    }

    [Fact]
    public void CompareTo_SameTimeSamePriority_ReturnsZero()
    {
        var a = new EventNote(null, null, null, null, T(5), 3, null);
        var b = new EventNote(null, null, null, null, T(5), 3, null);
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_SameTime_HigherPriorityFirst()
    {
        // Higher int priority value = should sort before lower int priority value.
        // CompareTo returns negative when "this" should come first.
        var highPri = new EventNote(null, null, null, null, T(5), 10, null);
        var lowPri  = new EventNote(null, null, null, null, T(5), 1, null);
        Assert.True(highPri.CompareTo(lowPri) < 0);
    }

    [Fact]
    public void Equals_SameFields_ReturnsTrue()
    {
        var t = T(5);
        var a = new EventNote(null, null, null, null, t, 3, null);
        var b = new EventNote(null, null, null, null, t, 3, null);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentTime_ReturnsFalse()
    {
        var a = new EventNote(null, null, null, null, T(5), 0, null);
        var b = new EventNote(null, null, null, null, T(6), 0, null);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_EqualNotes_SameHashCode()
    {
        var t = T(5);
        var a = new EventNote(null, null, null, null, t, 3, null);
        var b = new EventNote(null, null, null, null, t, 3, null);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ClearForPool_NullsAllReferences()
    {
        var e1 = new TestEntity(_model,"E1");
        var note = new EventNote(e1, null, null, null, T(5), 0, e1);
        note.ClearForPool();

        Assert.Null(note.getEntity1());
        Assert.Null(note.getEntity2());
        Assert.Null(note.getEntity3());
        Assert.Null(note.getEvent());
        Assert.Null(note.getTime());
        Assert.Null(note.getSource());
        Assert.False(note.isConnected());
    }

    [Fact]
    public void Reset_ReInitializesFields()
    {
        var e1  = new TestEntity(_model,"E1");
        var note = new EventNote(null, null, null, null, T(1), 0, null);
        note.ClearForPool();

        var t2 = T(99);
        note.Reset(e1, null, null, null, t2, 5, e1);

        Assert.Same(e1, note.getEntity1());
        Assert.Null(note.getEntity2());
        Assert.Null(note.getEntity3());
        Assert.Null(note.getEvent());
        Assert.Equal(t2.getTimeInEpsilon(), note.getTime().getTimeInEpsilon());
        Assert.Equal(5, note.getPriority());
        Assert.Same(e1, note.getSource());
        Assert.False(note.isConnected());
    }

    [Fact]
    public void Copy_ProducesIndependentObject()
    {
        var src  = new TestEntity(_model,"Src");
        var t    = T(10);
        var note = new EventNote(null, null, null, null, t, 2, src);
        var copy = note.copy(src);

        Assert.NotSame(note, copy);
        // Changing time on original should not affect copy
        note.setTime(T(999));
        Assert.Equal(t.getTimeInEpsilon(), copy.getTime().getTimeInEpsilon());
    }
}

// ── EventTreeListTests ─────────────────────────────────────────────────────

public class EventTreeListTests : IDisposable
{
    private readonly Experiment _experiment;
    private readonly HarnessModel _model;

    public EventTreeListTests()
    {
        (_model, _experiment) = SimTestHarness.Create();
    }

    public void Dispose() => _experiment.finish();

    private TimeInstant T(long seconds) => new TimeInstant(seconds, TimeUnit.SECONDS);

    private EventNote MakeNote(long seconds, int priority = 0)
        => new EventNote(null, null, null, null, T(seconds), priority, null);

    private int ListSize(EventTreeList list) => list.eTreeList.size();

    [Fact]
    public void NewList_IsEmpty_True()
    {
        var list = new EventTreeList();
        Assert.True(list.isEmpty());
    }

    [Fact]
    public void NewList_Size_IsZero()
    {
        var list = new EventTreeList();
        Assert.Equal(0, ListSize(list));
    }

    [Fact]
    public void Insert_OneNote_IsNotEmpty()
    {
        var list = new EventTreeList();
        list.insert(MakeNote(5));
        Assert.False(list.isEmpty());
    }

    [Fact]
    public void Insert_OneNote_Size_IsOne()
    {
        var list = new EventTreeList();
        list.insert(MakeNote(5));
        Assert.Equal(1, ListSize(list));
    }

    [Fact]
    public void Insert_OneNote_FirstNote_ReturnsThatNote()
    {
        var list = new EventTreeList();
        var note = MakeNote(5);
        list.insert(note);
        Assert.Same(note, list.firstNote());
    }

    [Fact]
    public void Insert_TwoNotes_EarlierTimeFirst()
    {
        var list  = new EventTreeList();
        var late  = MakeNote(10);
        var early = MakeNote(5);
        list.insert(late);
        list.insert(early);
        Assert.Same(early, list.firstNote());
    }

    [Fact]
    public void Insert_SameTime_HigherPriorityFirst()
    {
        var list    = new EventTreeList();
        var lowPri  = MakeNote(5, priority: 1);
        var highPri = MakeNote(5, priority: 10);
        list.insert(lowPri);
        list.insert(highPri);
        Assert.Same(highPri, list.firstNote());
    }

    [Fact]
    public void InsertAsFirst_WithLaterTime_ClampsTimeToExistingFirst()
    {
        var list   = new EventTreeList();
        var first  = MakeNote(10);
        list.insert(first);

        var newNote = MakeNote(20);
        list.insertAsFirst(newNote);

        // New note's time should be clamped to current-first's time (t=10)
        Assert.Equal(T(10).getTimeInEpsilon(), list.firstNote().getTime().getTimeInEpsilon());
    }

    [Fact]
    public void Remove_ExistingNote_SizeDecreases()
    {
        var list = new EventTreeList();
        var n1   = MakeNote(5);
        var n2   = MakeNote(10);
        list.insert(n1);
        list.insert(n2);
        list.remove(n1);
        Assert.Equal(1, ListSize(list));
    }

    [Fact]
    public void Remove_ExistingNote_FirstNoteChanges()
    {
        var list = new EventTreeList();
        var n1   = MakeNote(5);
        var n2   = MakeNote(10);
        list.insert(n1);
        list.insert(n2);
        list.remove(n1);
        Assert.Same(n2, list.firstNote());
    }

    [Fact]
    public void RemoveFirst_ReturnsNote()
    {
        var list = new EventTreeList();
        var n    = MakeNote(5);
        list.insert(n);
        var removed = list.removeFirst();
        Assert.Same(n, removed);
    }

    [Fact]
    public void RemoveFirst_DecreasesSize()
    {
        var list = new EventTreeList();
        list.insert(MakeNote(5));
        list.insert(MakeNote(10));
        list.removeFirst();
        Assert.Equal(1, ListSize(list));
    }

    [Fact]
    public void RemoveFirst_Empty_ReturnsNull()
    {
        var list = new EventTreeList();
        Assert.Null(list.removeFirst());
    }

    [Fact]
    public void FirstNote_PeekOnly_DoesNotRemoveNote()
    {
        var list = new EventTreeList();
        var n    = MakeNote(5);
        list.insert(n);

        _ = list.firstNote();
        _ = list.firstNote();

        Assert.Equal(1, ListSize(list));
        Assert.Same(n, list.firstNote());
    }
}
