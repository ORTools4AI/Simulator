// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Tests.core.simulator;

public class SimClockTests : IDisposable
{
    private readonly Experiment _exp;

    public SimClockTests()
    {
        _exp = new Experiment($"SimClockTest-{Guid.NewGuid():N}", false);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Constructor_AppendsClock_ToName()
    {
        var clock = new SimClock("Sim");
        Assert.Equal("Sim_clock", clock.getName());
    }

    [Fact]
    public void Constructor_InitialTime_IsZero()
    {
        var clock = new SimClock("C");
        Assert.Equal(0L, clock.getTime().getTimeInEpsilon());
    }

    [Fact]
    public void SetTime_FutureTime_UpdatesClock()
    {
        var clock = new SimClock("C");
        var t = new TimeInstant(5L, TimeUnit.SECONDS);
        clock.setTime(t);
        Assert.Equal(5_000_000L, clock.getTime().getTimeInEpsilon());
    }

    [Fact]
    public void SetTime_PastTime_IsIgnored()
    {
        var clock = new SimClock("C");
        var future = new TimeInstant(10L, TimeUnit.SECONDS);
        clock.setTime(future);

        var past = new TimeInstant(3L, TimeUnit.SECONDS);
        clock.setTime(past);

        // clock should still be at 10s, not 3s
        Assert.Equal(10_000_000L, clock.getTime().getTimeInEpsilon());
    }

    [Fact]
    public void SetTime_SameTime_IsIgnored()
    {
        var clock = new SimClock("C");
        var t = new TimeInstant(5L, TimeUnit.SECONDS);
        clock.setTime(t);
        clock.setTime(new TimeInstant(5L, TimeUnit.SECONDS));
        Assert.Equal(5_000_000L, clock.getTime().getTimeInEpsilon());
    }

    [Fact]
    public void SetInitTime_OverridesTime()
    {
        var clock = new SimClock("C");
        clock.setTime(new TimeInstant(10L, TimeUnit.SECONDS));
        clock.setInitTime(new TimeInstant(2L, TimeUnit.SECONDS));
        Assert.Equal(2_000_000L, clock.getTime().getTimeInEpsilon());
    }

    [Fact]
    public void ToString_ReturnsSameAsGetName()
    {
        var clock = new SimClock("MyModel");
        Assert.Equal(clock.getName(), clock.toString());
    }

    [Fact]
    public void SetTime_MultipleAdvances_RetainsLatest()
    {
        var clock = new SimClock("C");
        clock.setTime(new TimeInstant(1L, TimeUnit.SECONDS));
        clock.setTime(new TimeInstant(5L, TimeUnit.SECONDS));
        clock.setTime(new TimeInstant(3L, TimeUnit.SECONDS)); // ignored
        clock.setTime(new TimeInstant(10L, TimeUnit.SECONDS));
        Assert.Equal(10_000_000L, clock.getTime().getTimeInEpsilon());
    }
}
