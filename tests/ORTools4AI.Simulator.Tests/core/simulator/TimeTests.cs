// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.engine;
using DesmojTimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.Tests.core.simulator;

// ---------------------------------------------------------------------------
// TimeInstant tests
// ---------------------------------------------------------------------------

public class TimeInstantTests : IDisposable
{
    private readonly Experiment _experiment;

    public TimeInstantTests()
    {
        _experiment = new Experiment($"TimeInstantTest-{Guid.NewGuid():N}", false);
    }

    public void Dispose() => _experiment.finish();

    [Fact]
    public void Constructor_LongWithUnit_StoresCorrectEpsilons()
    {
        var t = new TimeInstant(5L, TimeUnit.SECONDS);
        Assert.Equal(5_000_000L, t.getTimeInEpsilon());
    }

    [Fact]
    public void Constructor_DoubleWithUnit_StoresCorrectEpsilons()
    {
        var t = new TimeInstant(2.5, TimeUnit.SECONDS);
        Assert.Equal(2_500_000L, t.getTimeInEpsilon());
    }

    [Fact]
    public void Constructor_LongReferenceUnit_UsesSeconds()
    {
        var t = new TimeInstant(3L);
        Assert.Equal(3_000_000L, t.getTimeInEpsilon());
    }

    [Fact]
    public void Constructor_LongNullUnit_ThrowsSimAbortedException()
    {
        Assert.Throws<SimAbortedException>(() => new TimeInstant(1L, null!));
    }

    [Fact]
    public void Constructor_DoubleNullUnit_ThrowsSimAbortedException()
    {
        Assert.Throws<SimAbortedException>(() => new TimeInstant(1.0, null!));
    }

    [Fact]
    public void GetTimeTruncated_CoarserUnit_Truncates()
    {
        // 1_500_000 microseconds = 1.5 seconds, truncated → 1
        var t = new TimeInstant(1_500_000L, TimeUnit.MICROSECONDS);
        Assert.Equal(1L, t.getTimeTruncated(TimeUnit.SECONDS));
    }

    [Fact]
    public void GetTimeRounded_CoarserUnit_RoundsHalfUp()
    {
        // 1_500_000 microseconds = exactly 1.5 seconds → rounds up to 2
        var t = new TimeInstant(1_500_000L, TimeUnit.MICROSECONDS);
        Assert.Equal(2L, t.getTimeRounded(TimeUnit.SECONDS));
    }

    [Fact]
    public void GetTimeRounded_BelowHalf_RoundsDown()
    {
        // 1_499_999 microseconds < 1.5 seconds → rounds down to 1
        var t = new TimeInstant(1_499_999L, TimeUnit.MICROSECONDS);
        Assert.Equal(1L, t.getTimeRounded(TimeUnit.SECONDS));
    }

    [Fact]
    public void CompareTo_EarlierTime_ReturnsNegative()
    {
        var a = new TimeInstant(1L, TimeUnit.SECONDS);
        var b = new TimeInstant(2L, TimeUnit.SECONDS);
        Assert.True(a.compareTo(b) < 0);
    }

    [Fact]
    public void CompareTo_SameTime_ReturnsZero()
    {
        var a = new TimeInstant(5L, TimeUnit.SECONDS);
        var b = new TimeInstant(5L, TimeUnit.SECONDS);
        Assert.Equal(0, a.compareTo(b));
    }

    [Fact]
    public void CompareTo_LaterTime_ReturnsPositive()
    {
        var a = new TimeInstant(3L, TimeUnit.SECONDS);
        var b = new TimeInstant(1L, TimeUnit.SECONDS);
        Assert.True(a.compareTo(b) > 0);
    }

    [Fact]
    public void Equals_SameEpsilonValue_ReturnsTrue()
    {
        var a = new TimeInstant(5L, TimeUnit.SECONDS);
        var b = new TimeInstant(5_000_000L, TimeUnit.MICROSECONDS);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new TimeInstant(1L, TimeUnit.SECONDS);
        var b = new TimeInstant(2L, TimeUnit.SECONDS);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_EqualObjects_SameHash()
    {
        var a = new TimeInstant(5L, TimeUnit.SECONDS);
        var b = new TimeInstant(5_000_000L, TimeUnit.MICROSECONDS);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void IsBefore_WhenBefore_ReturnsTrue()
    {
        var a = new TimeInstant(1L, TimeUnit.SECONDS);
        var b = new TimeInstant(2L, TimeUnit.SECONDS);
        Assert.True(TimeInstant.isBefore(a, b));
    }

    [Fact]
    public void IsBefore_WhenAfter_ReturnsFalse()
    {
        var a = new TimeInstant(3L, TimeUnit.SECONDS);
        var b = new TimeInstant(1L, TimeUnit.SECONDS);
        Assert.False(TimeInstant.isBefore(a, b));
    }

    [Fact]
    public void IsAfter_WhenAfter_ReturnsTrue()
    {
        var a = new TimeInstant(5L, TimeUnit.SECONDS);
        var b = new TimeInstant(2L, TimeUnit.SECONDS);
        Assert.True(TimeInstant.isAfter(a, b));
    }

    [Fact]
    public void IsAfterOrEqual_WhenEqual_ReturnsTrue()
    {
        var a = new TimeInstant(3L, TimeUnit.SECONDS);
        var b = new TimeInstant(3L, TimeUnit.SECONDS);
        Assert.True(TimeInstant.isAfterOrEqual(a, b));
    }

    [Fact]
    public void IsBeforeOrEqual_WhenEqual_ReturnsTrue()
    {
        var a = new TimeInstant(3L, TimeUnit.SECONDS);
        var b = new TimeInstant(3L, TimeUnit.SECONDS);
        Assert.True(TimeInstant.isBeforeOrEqual(a, b));
    }

    [Fact]
    public void IsEqual_WhenEqual_ReturnsTrue()
    {
        var a = new TimeInstant(7L, TimeUnit.SECONDS);
        var b = new TimeInstant(7L, TimeUnit.SECONDS);
        Assert.True(TimeInstant.isEqual(a, b));
    }

    [Fact]
    public void IsEqual_WhenNotEqual_ReturnsFalse()
    {
        var a = new TimeInstant(7L, TimeUnit.SECONDS);
        var b = new TimeInstant(8L, TimeUnit.SECONDS);
        Assert.False(TimeInstant.isEqual(a, b));
    }
}

// ---------------------------------------------------------------------------
// TimeSpan tests
// ---------------------------------------------------------------------------

public class TimeSpanTests : IDisposable
{
    private readonly Experiment _experiment;

    public TimeSpanTests()
    {
        _experiment = new Experiment($"TimeSpanTest-{Guid.NewGuid():N}", false);
    }

    public void Dispose() => _experiment.finish();

    [Fact]
    public void Constructor_LongNegative_ThrowsSimAbortedException()
    {
        Assert.Throws<SimAbortedException>(() => new DesmojTimeSpan(-1L, TimeUnit.SECONDS));
    }

    [Fact]
    public void Constructor_LongNullUnit_ThrowsSimAbortedException()
    {
        Assert.Throws<SimAbortedException>(() => new DesmojTimeSpan(1L, null!));
    }

    [Fact]
    public void Constructor_LongZero_IsZero()
    {
        var s = new DesmojTimeSpan(0L, TimeUnit.SECONDS);
        Assert.True(s.isZero());
    }

    [Fact]
    public void Constructor_LongPositive_StoresCorrectEpsilons()
    {
        var s = new DesmojTimeSpan(2L, TimeUnit.SECONDS);
        Assert.Equal(2_000_000L, s.getTimeInEpsilon());
    }

    [Fact]
    public void Constructor_DoubleNegative_ThrowsSimAbortedException()
    {
        Assert.Throws<SimAbortedException>(() => new DesmojTimeSpan(-0.5, TimeUnit.SECONDS));
    }

    [Fact]
    public void Constructor_DoubleNullUnit_ThrowsSimAbortedException()
    {
        Assert.Throws<SimAbortedException>(() => new DesmojTimeSpan(1.0, null!));
    }

    [Fact]
    public void Constructor_DoubleZero_IsZero()
    {
        var s = new DesmojTimeSpan(0.0, TimeUnit.SECONDS);
        Assert.True(s.isZero());
    }

    [Fact]
    public void SubEpsilonPositiveDouble_ClampsToOneEpsilon()
    {
        // 1e-10 seconds << 1 microsecond (epsilon); clamps to 1, not 0 (our fix)
        var s = new DesmojTimeSpan(1e-10, TimeUnit.SECONDS);
        Assert.Equal(1L, s.getTimeInEpsilon());
    }

    [Fact]
    public void ZERO_IsZero_True()
    {
        Assert.True(DesmojTimeSpan.ZERO.isZero());
    }

    [Fact]
    public void NonZero_IsZero_False()
    {
        var s = new DesmojTimeSpan(1L, TimeUnit.SECONDS);
        Assert.False(s.isZero());
    }

    [Fact]
    public void IsShorter_WhenShorter_ReturnsTrue()
    {
        var a = new DesmojTimeSpan(1L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        Assert.True(DesmojTimeSpan.isShorter(a, b));
    }

    [Fact]
    public void IsShorter_WhenLonger_ReturnsFalse()
    {
        var a = new DesmojTimeSpan(5L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(2L, TimeUnit.SECONDS);
        Assert.False(DesmojTimeSpan.isShorter(a, b));
    }

    [Fact]
    public void IsLonger_WhenLonger_ReturnsTrue()
    {
        var a = new DesmojTimeSpan(10L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        Assert.True(DesmojTimeSpan.isLonger(a, b));
    }

    [Fact]
    public void IsEqual_SameSpans_ReturnsTrue()
    {
        var a = new DesmojTimeSpan(4L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(4_000_000L, TimeUnit.MICROSECONDS);
        Assert.True(DesmojTimeSpan.isEqual(a, b));
    }

    [Fact]
    public void IsEqual_DifferentSpans_ReturnsFalse()
    {
        var a = new DesmojTimeSpan(1L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(2L, TimeUnit.SECONDS);
        Assert.False(DesmojTimeSpan.isEqual(a, b));
    }
}

// ---------------------------------------------------------------------------
// TimeOperations tests
// ---------------------------------------------------------------------------

public class TimeOperationsTests : IDisposable
{
    private readonly Experiment _experiment;

    public TimeOperationsTests()
    {
        _experiment = new Experiment($"TimeOpsTest-{Guid.NewGuid():N}", false);
    }

    public void Dispose() => _experiment.finish();

    [Fact]
    public void Add_TwoSpans_SumsEpsilons()
    {
        var a = new DesmojTimeSpan(2L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        Assert.Equal(5_000_000L, TimeOperations.add(a, b).getTimeInEpsilon());
    }

    [Fact]
    public void Add_SpanToInstant_AdvancesTime()
    {
        var instant = new TimeInstant(5L, TimeUnit.SECONDS);
        var span = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        Assert.Equal(8_000_000L, TimeOperations.add(instant, span).getTimeInEpsilon());
    }

    [Fact]
    public void Add_InstantAndSpan_Commutative()
    {
        var instant = new TimeInstant(4L, TimeUnit.SECONDS);
        var span = new DesmojTimeSpan(2L, TimeUnit.SECONDS);
        Assert.Equal(
            TimeOperations.add(instant, span).getTimeInEpsilon(),
            TimeOperations.add(span, instant).getTimeInEpsilon());
    }

    [Fact]
    public void Subtract_SpanFromInstant_RewindsTime()
    {
        var instant = new TimeInstant(5L, TimeUnit.SECONDS);
        var span = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        Assert.Equal(2_000_000L, TimeOperations.subtract(instant, span).getTimeInEpsilon());
    }

    [Fact]
    public void Subtract_SpanLargerThanInstant_ThrowsSimAbortedException()
    {
        var instant = new TimeInstant(1L, TimeUnit.SECONDS);
        var span = new DesmojTimeSpan(5L, TimeUnit.SECONDS);
        Assert.Throws<SimAbortedException>(() => TimeOperations.subtract(instant, span));
    }

    [Fact]
    public void Diff_Spans_AbsoluteDifference()
    {
        var a = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        var b = new DesmojTimeSpan(5L, TimeUnit.SECONDS);
        var diff1 = TimeOperations.diff(a, b).getTimeInEpsilon();
        var diff2 = TimeOperations.diff(b, a).getTimeInEpsilon();
        Assert.Equal(2_000_000L, diff1);
        Assert.Equal(diff1, diff2);
    }

    [Fact]
    public void Diff_Instants_AbsoluteDifference()
    {
        var a = new TimeInstant(3L, TimeUnit.SECONDS);
        var b = new TimeInstant(7L, TimeUnit.SECONDS);
        var diff1 = TimeOperations.diff(a, b).getTimeInEpsilon();
        var diff2 = TimeOperations.diff(b, a).getTimeInEpsilon();
        Assert.Equal(4_000_000L, diff1);
        Assert.Equal(diff1, diff2);
    }

    [Fact]
    public void Multiply_SpanByTwo_DoublesEpsilons()
    {
        var s = new DesmojTimeSpan(3L, TimeUnit.SECONDS);
        Assert.Equal(6_000_000L, TimeOperations.multiply(s, 2.0).getTimeInEpsilon());
    }

    [Fact]
    public void Divide_SpanBySpan_ReturnsRatio()
    {
        var dividend = new DesmojTimeSpan(6L, TimeUnit.SECONDS);
        var divisor = new DesmojTimeSpan(2L, TimeUnit.SECONDS);
        Assert.Equal(3.0, TimeOperations.divide(dividend, divisor), precision: 10);
    }

    [Fact]
    public void Divide_SpanByDouble_ReturnsScaledSpan()
    {
        var s = new DesmojTimeSpan(6L, TimeUnit.SECONDS);
        Assert.Equal(2_000_000L, TimeOperations.divide(s, 3.0).getTimeInEpsilon());
    }

    [Fact]
    public void GetEpsilon_Default_IsMicroseconds()
    {
        Assert.Equal(TimeUnit.MICROSECONDS, TimeOperations.getEpsilon());
    }

    [Fact]
    public void GetReferenceUnit_Default_IsSeconds()
    {
        Assert.Equal(TimeUnit.SECONDS, TimeOperations.getReferenceUnit());
    }
}
