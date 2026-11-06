// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.core.statistic;

namespace ORTools4AI.Simulator.Tests.core.statistic;

// ---------------------------------------------------------------------------
// Shared harness
// ---------------------------------------------------------------------------

internal sealed class StatModel : Model
{
    public StatModel() : base(null, "StatModel", false, false) { }
    public override string description() => "Statistic test model";
    public override void init() { }
    public override void doInitialSchedules() { }
}

// ---------------------------------------------------------------------------
// Tally tests
// ---------------------------------------------------------------------------

public class TallyTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly StatModel _model;
    private readonly Tally _tally;

    public TallyTests()
    {
        _exp = new Experiment($"TallyTest-{Guid.NewGuid():N}", false);
        _model = new StatModel();
        _model.connectToExperiment(_exp);
        _tally = new Tally(_model, "T", false, false);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void NoObservations_ObservationsCountIsZero()
        => Assert.Equal(0L, _tally.getObservations());

    [Fact]
    public void NoObservations_MeanIsUndefined()
    {
        // getMean() returns UNDEFINED (-1.0) when no data
        double mean = _tally.getMean();
        Assert.True(mean < 0 || double.IsNaN(mean));
    }

    [Fact]
    public void SingleObservation_MeanEqualsValue()
    {
        _tally.update(5.0);
        Assert.Equal(5.0, _tally.getMean(), precision: 6);
    }

    [Fact]
    public void MultipleObservations_MeanCorrect()
    {
        _tally.update(1.0);
        _tally.update(2.0);
        _tally.update(3.0);
        Assert.Equal(2.0, _tally.getMean(), precision: 6);
    }

    [Fact]
    public void MultipleObservations_ObservationsCountCorrect()
    {
        _tally.update(1.0);
        _tally.update(2.0);
        _tally.update(3.0);
        Assert.Equal(3L, _tally.getObservations());
    }

    [Fact]
    public void SingleObservation_MinEqualsValue()
    {
        _tally.update(7.0);
        Assert.Equal(7.0, _tally.getMinimum(), precision: 6);
    }

    [Fact]
    public void SingleObservation_MaxEqualsValue()
    {
        _tally.update(7.0);
        Assert.Equal(7.0, _tally.getMaximum(), precision: 6);
    }

    [Fact]
    public void MultipleObservations_MinTrackedCorrectly()
    {
        _tally.update(3.0);
        _tally.update(1.0);
        _tally.update(5.0);
        Assert.Equal(1.0, _tally.getMinimum(), precision: 6);
    }

    [Fact]
    public void MultipleObservations_MaxTrackedCorrectly()
    {
        _tally.update(3.0);
        _tally.update(1.0);
        _tally.update(5.0);
        Assert.Equal(5.0, _tally.getMaximum(), precision: 6);
    }

    [Fact]
    public void Reset_ZerosObservationCount()
    {
        _tally.update(1.0);
        _tally.update(2.0);
        _tally.reset();
        Assert.Equal(0L, _tally.getObservations());
    }

    [Fact]
    public void NegativeValues_Accepted()
    {
        _tally.update(-5.0);
        Assert.Equal(-5.0, _tally.getMean(), precision: 6);
    }

    [Fact]
    public void StdDev_TwoEqualValues_IsZero()
    {
        _tally.update(3.0);
        _tally.update(3.0);
        Assert.Equal(0.0, _tally.getStdDev(), precision: 6);
    }

    [Fact]
    public void StdDev_SingleObservation_IsUndefined()
    {
        _tally.update(1.0);
        double sd = _tally.getStdDev();
        Assert.True(sd < 0 || double.IsNaN(sd));
    }
}

// ---------------------------------------------------------------------------
// Accumulate tests
// Note: Accumulate.update() is a no-op when the experiment is not running,
// so these tests focus on initial state and reset behaviour. Time-weighted
// mean tests live in integration-level simulation tests.
// ---------------------------------------------------------------------------

public class AccumulateTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly StatModel _model;
    private readonly Accumulate _acc;

    public AccumulateTests()
    {
        _exp = new Experiment($"AccumTest-{Guid.NewGuid():N}", false);
        _model = new StatModel();
        _model.connectToExperiment(_exp);
        _acc = new Accumulate(_model, "A", false, false);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void InitialState_ObservationsIsZero()
        => Assert.Equal(0L, _acc.getObservations());

    [Fact]
    public void UpdateWhileNotRunning_IsNoOp()
    {
        // Experiment not started → update silently ignored
        _acc.update(10.0);
        Assert.Equal(0L, _acc.getObservations());
    }

    [Fact]
    public void Reset_DoesNotThrow()
    {
        // reset on an unused Accumulate should be safe
        var ex = Record.Exception(() => _acc.reset());
        Assert.Null(ex);
    }

    [Fact]
    public void Reset_AfterUpdateNoOp_StillZeroObservations()
    {
        _acc.update(1.0);
        _acc.reset();
        Assert.Equal(0L, _acc.getObservations());
    }

    [Fact]
    public void Creation_DoesNotThrow()
    {
        var ex = Record.Exception(() => new Accumulate(_model, "AccumNew", false, false));
        Assert.Null(ex);
    }
}

// ---------------------------------------------------------------------------
// Count tests
// ---------------------------------------------------------------------------

public class CountTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly StatModel _model;
    private readonly Count _count;

    public CountTests()
    {
        _exp = new Experiment($"CountTest-{Guid.NewGuid():N}", false);
        _model = new StatModel();
        _model.connectToExperiment(_exp);
        _count = new Count(_model, "C", false, false);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void InitialValue_IsZero()
        => Assert.Equal(0L, _count.getValue());

    [Fact]
    public void Update_NoArg_IncrementsBy1()
    {
        _count.update();
        Assert.Equal(1L, _count.getValue());
    }

    [Fact]
    public void Update_WithValue_IncreasesByThatValue()
    {
        _count.update(5L);
        Assert.Equal(5L, _count.getValue());
    }

    [Fact]
    public void MultipleUpdates_Accumulate()
    {
        _count.update(3L);
        _count.update(7L);
        Assert.Equal(10L, _count.getValue());
    }

    [Fact]
    public void Reset_ZerosValue()
    {
        _count.update(10L);
        _count.reset();
        Assert.Equal(0L, _count.getValue());
    }

    [Fact]
    public void Max_TrackedAfterIncrements()
    {
        _count.update(5L);
        _count.update(3L);
        Assert.Equal(8L, _count.getMaximum());
    }

    [Fact]
    public void ObservationsCount_MatchesUpdateCalls()
    {
        _count.update();
        _count.update();
        _count.update();
        Assert.Equal(3L, _count.getObservations());
    }
}

// ---------------------------------------------------------------------------
// BoolStatistic tests
// ---------------------------------------------------------------------------

public class BoolStatisticTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly StatModel _model;
    private readonly BoolStatistic _stat;

    public BoolStatisticTests()
    {
        _exp = new Experiment($"BoolStatTest-{Guid.NewGuid():N}", false);
        _model = new StatModel();
        _model.connectToExperiment(_exp);
        _stat = new BoolStatistic(_model, "BS", false, false);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void InitialState_ObservationsIsZero()
        => Assert.Equal(0L, _stat.getObservations());

    [Fact]
    public void InitialState_TrueObsIsZero()
        => Assert.Equal(0L, _stat.getTrueObs());

    [Fact]
    public void InitialState_TrueRatioIsUndefined()
        => Assert.True(_stat.getTrueRatio() < 0);

    [Fact]
    public void UpdateTrue_IncrementsTrueObs()
    {
        _stat.update(true);
        Assert.Equal(1L, _stat.getTrueObs());
    }

    [Fact]
    public void UpdateFalse_DoesNotIncrementTrueObs()
    {
        _stat.update(false);
        Assert.Equal(0L, _stat.getTrueObs());
    }

    [Fact]
    public void UpdateFalse_IncrementsObservations()
    {
        _stat.update(false);
        Assert.Equal(1L, _stat.getObservations());
    }

    [Fact]
    public void TrueRatio_AfterTrueAndFalse_IsHalf()
    {
        _stat.update(true);
        _stat.update(false);
        Assert.Equal(0.5, _stat.getTrueRatio(), precision: 6);
    }

    [Fact]
    public void TrueRatio_AllTrue_IsOne()
    {
        _stat.update(true);
        _stat.update(true);
        Assert.Equal(1.0, _stat.getTrueRatio(), precision: 6);
    }

    [Fact]
    public void Reset_ZerosAllCounts()
    {
        _stat.update(true);
        _stat.update(false);
        _stat.reset();
        Assert.Equal(0L, _stat.getObservations());
        Assert.Equal(0L, _stat.getTrueObs());
    }
}

// ---------------------------------------------------------------------------
// Histogram tests
// ---------------------------------------------------------------------------

public class HistogramTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly StatModel _model;

    public HistogramTests()
    {
        _exp = new Experiment($"HistogramTest-{Guid.NewGuid():N}", false);
        _model = new StatModel();
        _model.connectToExperiment(_exp);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var ex = Record.Exception(
            () => new Histogram(_model, "H", 0.0, 10.0, 5, false, false));
        Assert.Null(ex);
    }

    [Fact]
    public void InitialState_ZeroObservations()
    {
        var h = new Histogram(_model, "H", 0.0, 10.0, 5, false, false);
        Assert.Equal(0L, h.getObservations());
    }

    [Fact]
    public void Update_TracksMean()
    {
        var h = new Histogram(_model, "H", 0.0, 10.0, 5, false, false);
        h.update(2.0);
        h.update(4.0);
        Assert.Equal(3.0, h.getMean(), precision: 6);
    }

    [Fact]
    public void Update_TracksObservationCount()
    {
        var h = new Histogram(_model, "H2", 0.0, 10.0, 5, false, false);
        h.update(1.0);
        h.update(5.0);
        h.update(9.0);
        Assert.Equal(3L, h.getObservations());
    }

    [Fact]
    public void Reset_ZerosObservations()
    {
        var h = new Histogram(_model, "H3", 0.0, 10.0, 5, false, false);
        h.update(3.0);
        h.reset();
        Assert.Equal(0L, h.getObservations());
    }

    [Fact]
    public void IsCondensed_Default_IsFalse()
    {
        var h = new Histogram(_model, "H4", 0.0, 10.0, 5, false, false);
        Assert.False(h.isCondensed());
    }

    [Fact]
    public void SetCondensed_True_IsCondensedTrue()
    {
        var h = new Histogram(_model, "H5", 0.0, 10.0, 5, false, false);
        h.setCondensed(true);
        Assert.True(h.isCondensed());
    }
}
