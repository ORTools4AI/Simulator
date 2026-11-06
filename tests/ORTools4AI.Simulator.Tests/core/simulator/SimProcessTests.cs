// Modified for ORTools4AI.Simulator.

using System.Collections;
using System.Reflection;
using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.engine;
using Xunit;
using DesmojTimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.Tests.core.simulator;

// ---------------------------------------------------------------------------
// Test harness helpers
// ---------------------------------------------------------------------------

internal class TestModel : Model
{
    public TestModel() : base(null, "TestModel", false, false) { }
    public override string description() => "Test";
    public override void init() { }
    public override void doInitialSchedules() { }
}

internal class SpyProcess : SimProcess
{
    public List<string> Steps { get; } = new();
    private readonly int _suspendAfter;

    public SpyProcess(Model owner, int suspendAfter = 0)
        : base(owner, "SpyProcess", false)
    {
        _suspendAfter = suspendAfter;
    }

    protected override IEnumerable Run()
    {
        Steps.Add("started");
        for (int i = 0; i < _suspendAfter; i++)
        {
            Steps.Add($"step-{i}");
            yield return Suspend();
        }
        Steps.Add("finished");
    }
}

internal class ChainProcess : SimProcess
{
    public List<string> Steps { get; } = new();
    private readonly SimProcess? _child;

    public ChainProcess(Model owner, SimProcess? child = null)
        : base(owner, "ChainProcess", false) => _child = child;

    protected override IEnumerable Run()
    {
        Steps.Add("before-child");
        _child?.start();
        Steps.Add("after-child");
        yield return Suspend();
        Steps.Add("resumed");
    }
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

public class SimProcessTests : IDisposable
{
    private readonly Experiment _experiment;
    private readonly TestModel _model;

    public SimProcessTests()
    {
        _experiment = new Experiment($"SimProcessTest-{Guid.NewGuid():N}", false);
        _model = new TestModel();
        _model.connectToExperiment(_experiment);
    }

    public void Dispose() => _experiment.finish();

    private SpyProcess Process(int suspendAfter = 0) => new(_model, suspendAfter);

    // -----------------------------------------------------------------------
    // 1. Lifecycle / start()
    // -----------------------------------------------------------------------

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var spy = Process();
        spy.start();
        Assert.True(spy.isReady());
    }

    [Fact]
    public void Start_ExecutesLifeCycle_ToFirstYield_WhenSuspendAfterOne()
    {
        var spy = Process(suspendAfter: 2);
        spy.start();
        Assert.Contains("started", spy.Steps);
        Assert.Contains("step-0", spy.Steps);
        Assert.DoesNotContain("step-1", spy.Steps);
        Assert.DoesNotContain("finished", spy.Steps);
    }

    [Fact]
    public void Start_ExecutesFullLifeCycle_WhenNoYield()
    {
        var spy = Process(suspendAfter: 0);
        spy.start();
        Assert.Contains("started", spy.Steps);
        Assert.Contains("finished", spy.Steps);
    }

    [Fact]
    public void Resume_AdvancesLifeCycle_OneStepBeyondFirstYield()
    {
        var spy = Process(suspendAfter: 2);
        spy.start();
        Assert.DoesNotContain("step-1", spy.Steps);
        spy.resume();
        Assert.Contains("step-1", spy.Steps);
    }

    // -----------------------------------------------------------------------
    // 2. State flags
    // -----------------------------------------------------------------------

    [Fact]
    public void InitialState_AllFlagsAreFalse()
    {
        var spy = Process();
        Assert.False(spy.isBlocked());
        Assert.False(spy.isReady());
        Assert.False(spy.isTerminated());
        Assert.False(spy.isInterrupted());
        Assert.False(spy.isRepeating());
        Assert.False(spy.isComponent());
        Assert.True(spy.canCooperate());
    }

    [Fact]
    public void SetBlocked_TogglesIsBlocked()
    {
        var spy = Process();
        spy.setBlocked(true);
        Assert.True(spy.isBlocked());
        spy.setBlocked(false);
        Assert.False(spy.isBlocked());
    }

    [Fact]
    public void SetTerminated_TogglesIsTerminated()
    {
        var spy = Process();
        spy.setTerminated(true);
        Assert.True(spy.isTerminated());
        spy.setTerminated(false);
        Assert.False(spy.isTerminated());
    }

    [Fact]
    public void SetRepeating_TogglesIsRepeating()
    {
        var spy = Process();
        spy.setRepeating(true);
        Assert.True(spy.isRepeating());
        spy.setRepeating(false);
        Assert.False(spy.isRepeating());
    }

    [Fact]
    public void SetRunning_TogglesIsReady()
    {
        var spy = Process();
        spy.setRunning(true);
        Assert.True(spy.isReady());
        spy.setRunning(false);
        Assert.False(spy.isReady());
    }

    [Fact]
    public void SetSchedulingPriority_IsReflectedByGetter()
    {
        var spy = Process();
        spy.setSchedulingPriority(42);
        Assert.Equal(42, spy.getSchedulingPriority());
    }

    [Fact]
    public void DefaultSchedulingPriority_IsZero()
    {
        var spy = Process();
        Assert.Equal(0, spy.getSchedulingPriority());
    }

    [Fact]
    public void SetRealTimeConstraint_IsReflectedByGetter()
    {
        var spy = Process();
        spy.setRealTimeConstraint(999_000L);
        Assert.Equal(999_000L, spy.getRealTimeConstraint());
    }

    // -----------------------------------------------------------------------
    // 3. isInterrupted() / interrupt state
    // -----------------------------------------------------------------------

    [Fact]
    public void IsInterrupted_FalseInitially()
    {
        var spy = Process();
        Assert.False(spy.isInterrupted());
        Assert.Null(spy.getInterruptCode());
        Assert.Null(spy.getInterruptException());
    }

    [Fact]
    public void Interrupt_ValidCode_SetsIsInterruptedTrue()
    {
        var spy = Process();
        var code = new InterruptCode("Reason");
        spy.interrupt(code);
        Assert.True(spy.isInterrupted());
        Assert.Equal(code, spy.getInterruptCode());
    }

    [Fact]
    public void Interrupt_ValidException_SetsIsInterruptedTrue()
    {
        var spy = Process();
        var code = new InterruptCode("Reason");
        var irqEx = new InterruptException(code);
        spy.interrupt(irqEx);
        Assert.True(spy.isInterrupted());
        Assert.Equal(irqEx, spy.getInterruptException());
    }

    [Fact]
    public void ClearInterruptCode_AfterInterrupt_ResetsIsInterrupted()
    {
        var spy = Process();
        spy.interrupt(new InterruptCode("Reason"));
        Assert.True(spy.isInterrupted());
        spy.clearInterruptCode();
        Assert.False(spy.isInterrupted());
        Assert.Null(spy.getInterruptCode());
    }

    [Fact]
    public void IsInterrupted_TrueWhenBothCodeAndExceptionSet()
    {
        var spy = Process();
        var code = new InterruptCode("Code");
        spy.interrupt(code);
        // isInterrupted should be true when irqCode is non-null
        Assert.True(spy.isInterrupted());
    }

    // -----------------------------------------------------------------------
    // 4. interrupt() guard conditions
    // -----------------------------------------------------------------------

    [Fact]
    public void Interrupt_NullCode_IsIgnored()
    {
        var spy = Process();
        spy.interrupt((InterruptCode?)null);
        Assert.False(spy.isInterrupted());
    }

    [Fact]
    public void Interrupt_NullException_IsIgnored()
    {
        var spy = Process();
        spy.interrupt((InterruptException?)null);
        Assert.False(spy.isInterrupted());
    }

    [Fact]
    public void Interrupt_ExceptionWithNullCode_IsIgnored()
    {
        var spy = Process();
        spy.interrupt(new InterruptException(null!));
        Assert.False(spy.isInterrupted());
    }

    [Fact]
    public void Interrupt_BlockedProcess_IsIgnored()
    {
        var spy = Process();
        spy.setBlocked(true);
        spy.interrupt(new InterruptCode("Reason"));
        Assert.False(spy.isInterrupted());
    }

    [Fact]
    public void Interrupt_TerminatedProcess_IsIgnored()
    {
        var spy = Process();
        spy.setTerminated(true);
        spy.interrupt(new InterruptCode("Reason"));
        Assert.False(spy.isInterrupted());
    }

    [Fact]
    public void Interrupt_AlreadyInterrupted_SecondInterruptIsIgnored()
    {
        var spy = Process();
        var first = new InterruptCode("First");
        var second = new InterruptCode("Second");
        spy.interrupt(first);
        spy.interrupt(second);
        // second interrupt ignored; first code still set
        Assert.Equal(first, spy.getInterruptCode());
    }

    // -----------------------------------------------------------------------
    // 5. activate() guard conditions
    // -----------------------------------------------------------------------

    [Fact]
    public void Activate_WhenBlocked_ProcessRemainsUnscheduled()
    {
        var spy = Process();
        spy.setBlocked(true);
        spy.activate();
        Assert.False(spy.isScheduled());
    }

    [Fact]
    public void Activate_WithNullTimeSpan_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.activate((DesmojTimeSpan?)null));
        Assert.Null(ex);
        Assert.False(spy.isScheduled());
    }

    [Fact]
    public void Activate_WithNullTimeInstant_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.activate((TimeInstant?)null));
        Assert.Null(ex);
        Assert.False(spy.isScheduled());
    }

    [Fact]
    public void ActivateAfter_WithNullSchedulable_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.activateAfter(null));
        Assert.Null(ex);
    }

    [Fact]
    public void ActivateBefore_WithNullSchedulable_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.activateBefore(null));
        Assert.Null(ex);
    }

    // -----------------------------------------------------------------------
    // 6. hold() guard conditions
    // -----------------------------------------------------------------------

    [Fact]
    public void Hold_TimeSpan_WhenBlocked_DoesNotThrow()
    {
        var spy = Process();
        spy.setBlocked(true);
        var ex = Record.Exception(() => spy.hold(new DesmojTimeSpan(5)));
        Assert.Null(ex);
        Assert.False(spy.isScheduled());
    }

    [Fact]
    public void Hold_NullTimeSpan_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.hold((DesmojTimeSpan?)null));
        Assert.Null(ex);
    }

    [Fact]
    public void Hold_NullTimeInstant_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.hold((TimeInstant?)null));
        Assert.Null(ex);
    }

    // -----------------------------------------------------------------------
    // 7. cooperate() guard conditions
    // -----------------------------------------------------------------------

    [Fact]
    public void Cooperate_WhenTerminated_DoesNotThrow()
    {
        var spy = Process();
        spy.setTerminated(true);
        var ex = Record.Exception(() => spy.cooperate());
        Assert.Null(ex);
    }

    [Fact]
    public void Cooperate_WhenNotInSlaveQueue_DoesNotThrow()
    {
        // slaveWaitQueue is null by default; cooperate() warns and returns
        var spy = Process();
        var ex = Record.Exception(() => spy.cooperate());
        Assert.Null(ex);
    }

    // -----------------------------------------------------------------------
    // 8. resetMaster()
    // -----------------------------------------------------------------------

    [Fact]
    public void ResetMaster_WhenNoMaster_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.resetMaster());
        Assert.Null(ex);
        Assert.Null(spy.getMaster());
    }

    // -----------------------------------------------------------------------
    // 9. Resource management
    // -----------------------------------------------------------------------

    [Fact]
    public void ObtainResources_EmptyArray_IsIgnored()
    {
        var spy = Process();
        spy.obtainResources(Array.Empty<Resource>());
        Assert.Empty(spy.getUsedResources());
    }

    [Fact]
    public void ObtainResources_AddsToUsedList()
    {
        var spy = Process();
        var pool = new Res(_model, "Pool1", 5, false, false);
        var r1 = new Resource(_model, "R1", pool, false);
        var r2 = new Resource(_model, "R2", pool, false);
        spy.obtainResources(new[] { r1, r2 });
        Assert.Equal(2, spy.getUsedResources().Count);
        Assert.Contains(r1, spy.getUsedResources());
        Assert.Contains(r2, spy.getUsedResources());
    }

    [Fact]
    public void ReturnResources_ZeroCount_IsIgnoredAndReturnsNull()
    {
        var spy = Process();
        var pool = new Res(_model, "Pool2", 5, false, false);
        var r = new Resource(_model, "R3", pool, false);
        spy.obtainResources(new[] { r });

        var result = spy.returnResources(pool, 0);

        Assert.Null(result);
        Assert.Single(spy.getUsedResources()); // still holding the resource
    }

    [Fact]
    public void ReturnResources_NegativeCount_IsIgnoredAndReturnsNull()
    {
        var spy = Process();
        var pool = new Res(_model, "Pool3", 5, false, false);

        var result = spy.returnResources(pool, -1);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnResources_WhenNoneHeld_ReturnsNull()
    {
        var spy = Process();
        var pool = new Res(_model, "Pool4", 5, false, false);

        var result = spy.returnResources(pool, 1);

        Assert.Null(result);
    }

    [Fact]
    public void ReturnAllResources_WhenNoneHeld_DoesNotThrow()
    {
        var spy = Process();
        var ex = Record.Exception(() => spy.returnAllResources());
        Assert.Null(ex);
        Assert.Empty(spy.getUsedResources());
    }

    [Fact]
    public void GetUsedResources_ReturnsDefensiveCopy()
    {
        var spy = Process();
        var pool = new Res(_model, "Pool5", 5, false, false);
        var r = new Resource(_model, "R4", pool, false);
        spy.obtainResources(new[] { r });

        var copy1 = spy.getUsedResources();
        var copy2 = spy.getUsedResources();

        Assert.NotSame(copy1, copy2);
        Assert.Equal(copy1, copy2);
    }

    // -----------------------------------------------------------------------
    // 10. interruptDelayed schedules a delayed interrupt event
    // -----------------------------------------------------------------------

    [Fact]
    public void InterruptDelayed_TimeInstant_SchedulesEvent()
    {
        var spy = Process();
        var evt = spy.interruptDelayed(new TimeInstant(10));
        Assert.NotNull(evt);
        Assert.True(spy.isDelayedInterruptScheduled());
    }

    [Fact]
    public void InterruptDelayed_TimeSpan_SchedulesEvent()
    {
        var spy = Process();
        var evt = spy.interruptDelayed(new DesmojTimeSpan(10));
        Assert.NotNull(evt);
        Assert.True(spy.isDelayedInterruptScheduled());
    }

    // -----------------------------------------------------------------------
    // 11. isDelayedInterruptScheduled()
    // -----------------------------------------------------------------------

    [Fact]
    public void IsDelayedInterruptScheduled_FalseWhenNoneScheduled()
    {
        var spy = Process();
        Assert.False(spy.isDelayedInterruptScheduled());
    }

    // -----------------------------------------------------------------------
    // Helper: inject _master via reflection (cooperate() needs the event loop)
    // -----------------------------------------------------------------------

    private static void SetMaster(SimProcess slave, SimProcess master) =>
        typeof(SimProcess)
            .GetField("_master", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(slave, master);

    // -----------------------------------------------------------------------
    // 12. Independent lifecycles — two processes do not share step state
    // -----------------------------------------------------------------------

    [Fact]
    public void TwoProcesses_Start_TrackStepsIndependently()
    {
        var a = Process(suspendAfter: 2);
        var b = Process(suspendAfter: 2);
        a.start();
        b.start();

        Assert.Equal(new[] { "started", "step-0" }, a.Steps);
        Assert.Equal(new[] { "started", "step-0" }, b.Steps);
    }

    [Fact]
    public void Resume_ProcessA_DoesNotAdvanceProcessB()
    {
        var a = Process(suspendAfter: 2);
        var b = Process(suspendAfter: 2);
        a.start();
        b.start();

        a.resume(); // advances A to step-1

        Assert.Contains("step-1", a.Steps);
        Assert.DoesNotContain("step-1", b.Steps);
    }

    [Fact]
    public void Resume_ProcessB_DoesNotAdvanceProcessA()
    {
        var a = Process(suspendAfter: 2);
        var b = Process(suspendAfter: 2);
        a.start();
        b.start();

        b.resume(); // advances B to step-1

        Assert.Contains("step-1", b.Steps);
        Assert.DoesNotContain("step-1", a.Steps);
    }

    [Fact]
    public void TwoProcesses_FullyResumed_FinishIndependently()
    {
        var a = Process(suspendAfter: 1);
        var b = Process(suspendAfter: 1);
        a.start();
        b.start();
        a.resume(); // A finishes
        b.resume(); // B finishes

        Assert.Contains("finished", a.Steps);
        Assert.Contains("finished", b.Steps);
    }

    // -----------------------------------------------------------------------
    // 13. Process chain — A's lifeCycle() starts B
    // -----------------------------------------------------------------------

    [Fact]
    public void ChainProcess_Start_ExecutesChildLifeCycleBeforeYielding()
    {
        var child = Process(suspendAfter: 0); // child runs to completion
        var parent = new ChainProcess(_model, child);
        parent.start();

        Assert.Equal(new[] { "started", "finished" }, child.Steps);
        Assert.Contains("before-child", parent.Steps);
        Assert.Contains("after-child", parent.Steps);
        Assert.DoesNotContain("resumed", parent.Steps);
    }

    [Fact]
    public void ChainProcess_Resumed_CompletesParentLifeCycle()
    {
        var child = Process(suspendAfter: 0);
        var parent = new ChainProcess(_model, child);
        parent.start();
        parent.resume();

        Assert.Contains("resumed", parent.Steps);
    }

    [Fact]
    public void ChainProcess_WithSuspendingChild_ChildPausedAtFirstYield()
    {
        var child = Process(suspendAfter: 2); // child yields before finishing
        var parent = new ChainProcess(_model, child);
        parent.start();

        // Parent called child.start(), which ran child to its first yield
        Assert.Contains("started", child.Steps);
        Assert.Contains("step-0", child.Steps);
        Assert.DoesNotContain("finished", child.Steps);
        // Parent then continued past child.start() and hit its own yield
        Assert.Contains("after-child", parent.Steps);
    }

    // -----------------------------------------------------------------------
    // 14. Cross-process interrupt — one interrupt does not affect the other
    // -----------------------------------------------------------------------

    [Fact]
    public void Interrupt_ProcessA_DoesNotAffectProcessB()
    {
        var a = Process();
        var b = Process();
        a.interrupt(new InterruptCode("Reason"));

        Assert.True(a.isInterrupted());
        Assert.False(b.isInterrupted());
    }

    [Fact]
    public void TwoProcesses_InterruptedWithDifferentCodes_HoldOwnCodes()
    {
        var a = Process();
        var b = Process();
        var codeA = new InterruptCode("CodeA");
        var codeB = new InterruptCode("CodeB");

        a.interrupt(codeA);
        b.interrupt(codeB);

        Assert.Equal(codeA, a.getInterruptCode());
        Assert.Equal(codeB, b.getInterruptCode());
    }

    // -----------------------------------------------------------------------
    // 15. Master/slave interaction (master injected via reflection)
    // -----------------------------------------------------------------------

    [Fact]
    public void CanCooperate_FalseWhenMasterIsSet()
    {
        var master = Process();
        var slave = Process();
        SetMaster(slave, master);

        Assert.False(slave.canCooperate());
    }

    [Fact]
    public void CanCooperate_TrueAfterResetMaster()
    {
        var master = Process();
        var slave = Process();
        SetMaster(slave, master);
        slave.resetMaster();

        Assert.True(slave.canCooperate());
    }

    [Fact]
    public void Interrupt_SlaveWithMaster_PropagatesInterruptToMaster()
    {
        var master = Process();
        var slave = Process();
        SetMaster(slave, master);

        var code = new InterruptCode("Propagated");
        slave.interrupt(code);

        Assert.True(master.isInterrupted());
        Assert.Equal(code, master.getInterruptCode());
    }

    [Fact]
    public void Interrupt_SlaveWithMaster_AlsoInterruptsSlave()
    {
        var master = Process();
        var slave = Process();
        SetMaster(slave, master);

        var code = new InterruptCode("Propagated");
        slave.interrupt(code);

        Assert.True(slave.isInterrupted());
    }

    [Fact]
    public void ResetMaster_CopiesMastersInterruptCodeToSlave()
    {
        var master = Process();
        var slave = Process();
        var code = new InterruptCode("MasterCode");
        // interrupt the master first, then wire slave -> master
        master.interrupt(code);
        SetMaster(slave, master);

        slave.resetMaster();

        // slave should inherit master's interrupt code and master field clears
        Assert.Equal(code, slave.getInterruptCode());
        Assert.Null(slave.getMaster());
    }

    [Fact]
    public void ResetMaster_WhenMasterNotInterrupted_SlaveKeepsNoCode()
    {
        var master = Process();
        var slave = Process();
        SetMaster(slave, master);

        slave.resetMaster();

        Assert.Null(slave.getInterruptCode());
        Assert.Null(slave.getMaster());
    }

    // -----------------------------------------------------------------------
    // 16. Shared resource pool — two processes hold from the same pool
    // -----------------------------------------------------------------------

    [Fact]
    public void TwoProcesses_ObtainFromSamePool_BothHoldResources()
    {
        var pool = new Res(_model, "SharedPool", 10, false, false);
        var r1 = new Resource(_model, "SR1", pool, false);
        var r2 = new Resource(_model, "SR2", pool, false);
        var p1 = Process();
        var p2 = Process();

        p1.obtainResources(new[] { r1 });
        p2.obtainResources(new[] { r2 });

        Assert.Single(p1.getUsedResources());
        Assert.Single(p2.getUsedResources());
        Assert.Contains(r1, p1.getUsedResources());
        Assert.Contains(r2, p2.getUsedResources());
    }

    [Fact]
    public void ReturnAllResources_P1_DoesNotAffectP2Resources()
    {
        var pool = new Res(_model, "SharedPool2", 10, false, false);
        var r1 = new Resource(_model, "SR3", pool, false);
        var r2 = new Resource(_model, "SR4", pool, false);
        var p1 = Process();
        var p2 = Process();
        p1.obtainResources(new[] { r1 });
        p2.obtainResources(new[] { r2 });

        p1.returnAllResources();

        Assert.Empty(p1.getUsedResources());
        Assert.Single(p2.getUsedResources());
        Assert.Contains(r2, p2.getUsedResources());
    }

    [Fact]
    public void ClearInterruptCode_P1_DoesNotAffectP2InterruptState()
    {
        var p1 = Process();
        var p2 = Process();
        p1.interrupt(new InterruptCode("Code1"));
        p2.interrupt(new InterruptCode("Code2"));

        p1.clearInterruptCode();

        Assert.False(p1.isInterrupted());
        Assert.True(p2.isInterrupted());
    }
}
