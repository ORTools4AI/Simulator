// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Tests.core.dist;

// ---------------------------------------------------------------------------
// Shared harness
// ---------------------------------------------------------------------------

internal sealed class DistModel : Model
{
    public DistModel() : base(null, "DistModel", false, false) { }
    public override string description() => "Distribution test model";
    public override void init() { }
    public override void doInitialSchedules() { }
}

// ---------------------------------------------------------------------------
// ContDistExponential tests
// ---------------------------------------------------------------------------

public class ExponentialDistTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly DistModel _model;

    public ExponentialDistTests()
    {
        _exp = new Experiment($"ExpDistTest-{Guid.NewGuid():N}", false);
        _model = new DistModel();
        _model.connectToExperiment(_exp);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Sample_WithNonNegative_AllSamplesNonNegative()
    {
        var dist = new ContDistExponential(_model, "Exp", 1.0, false, false);
        dist.setNonNegative(true);
        for (int i = 0; i < 100; i++)
            Assert.True(dist.sample() >= 0.0);
    }

    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var dist = new ContDistExponential(_model, "Exp1", 2.0, false, false);
        dist.setNonNegative(true);

        dist.setSeed(12345L);
        var seq1 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();

        dist.setSeed(12345L);
        var seq2 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();

        Assert.Equal(seq1, seq2);
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentFirstSample()
    {
        var dist = new ContDistExponential(_model, "Exp2", 1.0, false, false);
        dist.setNonNegative(true);

        dist.setSeed(1L);
        double s1 = dist.sample();

        dist.setSeed(9999L);
        double s2 = dist.sample();

        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void LargeSampleSet_AllNonNegative_WithNonNegativeFlag()
    {
        var dist = new ContDistExponential(_model, "ExpLarge", 0.5, false, false);
        dist.setNonNegative(true);
        for (int i = 0; i < 1000; i++)
            Assert.True(dist.sample() >= 0.0, $"Sample {i} was negative");
    }
}

// ---------------------------------------------------------------------------
// ContDistUniform tests
// ---------------------------------------------------------------------------

public class UniformDistTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly DistModel _model;

    public UniformDistTests()
    {
        _exp = new Experiment($"UniDistTest-{Guid.NewGuid():N}", false);
        _model = new DistModel();
        _model.connectToExperiment(_exp);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Samples_AllWithinBounds()
    {
        var dist = new ContDistUniform(_model, "Uni", 2.0, 8.0, false, false);
        for (int i = 0; i < 100; i++)
        {
            double s = dist.sample();
            Assert.InRange(s, 2.0, 8.0);
        }
    }

    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var dist = new ContDistUniform(_model, "Uni2", 0.0, 1.0, false, false);

        dist.setSeed(42L);
        var seq1 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();

        dist.setSeed(42L);
        var seq2 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();

        Assert.Equal(seq1, seq2);
    }

    [Fact]
    public void SwappedBounds_StillProducesValidRange()
    {
        // Constructor swaps min/max if passed in wrong order
        var dist = new ContDistUniform(_model, "UniSwap", 10.0, 1.0, false, false);
        for (int i = 0; i < 50; i++)
        {
            double s = dist.sample();
            Assert.InRange(s, 1.0, 10.0);
        }
    }

    [Fact]
    public void EqualBounds_AlwaysReturnsSameValue()
    {
        var dist = new ContDistUniform(_model, "UniConst", 5.0, 5.0, false, false);
        for (int i = 0; i < 10; i++)
            Assert.Equal(5.0, dist.sample(), precision: 9);
    }
}

// ---------------------------------------------------------------------------
// DiscreteDistPoisson tests
// ---------------------------------------------------------------------------

public class PoissonDistTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly DistModel _model;

    public PoissonDistTests()
    {
        _exp = new Experiment($"PoissonTest-{Guid.NewGuid():N}", false);
        _model = new DistModel();
        _model.connectToExperiment(_exp);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Samples_AllNonNegative()
    {
        var dist = new DiscreteDistPoisson(_model, "Poisson", 3.0, false, false);
        for (int i = 0; i < 100; i++)
            Assert.True(dist.sample() >= 0L);
    }

    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var dist = new DiscreteDistPoisson(_model, "Poisson2", 5.0, false, false);

        dist.setSeed(777L);
        var seq1 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();

        dist.setSeed(777L);
        var seq2 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();

        Assert.Equal(seq1, seq2);
    }

    [Fact]
    public void LargeSampleSet_AllNonNegative()
    {
        var dist = new DiscreteDistPoisson(_model, "PoissonLarge", 2.0, false, false);
        for (int i = 0; i < 500; i++)
            Assert.True(dist.sample() >= 0L, $"Sample {i} was negative");
    }

    [Fact]
    public void ObservationCount_TracksCorrectly()
    {
        var dist = new DiscreteDistPoisson(_model, "PoissonObs", 1.0, false, false);
        for (int i = 0; i < 5; i++)
            dist.sample();
        Assert.Equal(5L, dist.getObservations());
    }
}

// ---------------------------------------------------------------------------
// LinearCongruentialRandomGenerator tests
// ---------------------------------------------------------------------------

public class LinearCongruentialRngTests
{
    [Fact]
    public void DefaultConstructor_ProducesValuesInUnitInterval()
    {
        var rng = new LinearCongruentialRandomGenerator();
        for (int i = 0; i < 100; i++)
        {
            double v = rng.nextDouble();
            Assert.InRange(v, 0.0, 1.0);
        }
    }

    [Fact]
    public void SeededConstructor_SameSeed_ProducesSameSequence()
    {
        var rng1 = new LinearCongruentialRandomGenerator(12345L);
        var rng2 = new LinearCongruentialRandomGenerator(12345L);
        for (int i = 0; i < 20; i++)
            Assert.Equal(rng1.nextDouble(), rng2.nextDouble());
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentFirstValue()
    {
        var rng1 = new LinearCongruentialRandomGenerator(1L);
        var rng2 = new LinearCongruentialRandomGenerator(9999L);
        Assert.NotEqual(rng1.nextDouble(), rng2.nextDouble());
    }

    [Fact]
    public void SetSeed_ResetsToSameSequence()
    {
        var rng = new LinearCongruentialRandomGenerator(100L);
        double first1 = rng.nextDouble();
        rng.setSeed(100L);
        double first2 = rng.nextDouble();
        Assert.Equal(first1, first2);
    }

    [Fact]
    public void DefaultSeed42_IsDeterministic()
    {
        var rng1 = new LinearCongruentialRandomGenerator();
        var rng2 = new LinearCongruentialRandomGenerator();
        Assert.Equal(rng1.nextDouble(), rng2.nextDouble());
    }
}

// ---------------------------------------------------------------------------
// DiscreteDistUniform tests
// ---------------------------------------------------------------------------

public class DiscreteDistUniformTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly DistModel _model;

    public DiscreteDistUniformTests()
    {
        _exp = new Experiment($"DiscreteUniformTest-{Guid.NewGuid():N}", false);
        _model = new DistModel();
        _model.connectToExperiment(_exp);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void Samples_AllWithinBounds()
    {
        var dist = new DiscreteDistUniform(_model, "DU", 1L, 6L, false, false);
        for (int i = 0; i < 100; i++)
        {
            long s = dist.sample();
            Assert.InRange(s, 1L, 6L);
        }
    }

    [Fact]
    public void SwappedBounds_StillInValidRange()
    {
        var dist = new DiscreteDistUniform(_model, "DUSwap", 10L, 1L, false, false);
        for (int i = 0; i < 50; i++)
            Assert.InRange(dist.sample(), 1L, 10L);
    }

    [Fact]
    public void GetLower_ReturnsMinBound()
    {
        var dist = new DiscreteDistUniform(_model, "DU2", 3L, 9L, false, false);
        Assert.Equal(3L, dist.getLower());
    }

    [Fact]
    public void GetUpper_ReturnsMaxBound()
    {
        var dist = new DiscreteDistUniform(_model, "DU3", 3L, 9L, false, false);
        Assert.Equal(9L, dist.getUpper());
    }

    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var dist = new DiscreteDistUniform(_model, "DU4", 1L, 100L, false, false);
        dist.setSeed(777L);
        var seq1 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();
        dist.setSeed(777L);
        var seq2 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();
        Assert.Equal(seq1, seq2);
    }
}

// ---------------------------------------------------------------------------
// BoolDistBernoulli tests
// ---------------------------------------------------------------------------

public class BoolDistBernoulliTests : IDisposable
{
    private readonly Experiment _exp;
    private readonly DistModel _model;

    public BoolDistBernoulliTests()
    {
        _exp = new Experiment($"BernoulliTest-{Guid.NewGuid():N}", false);
        _model = new DistModel();
        _model.connectToExperiment(_exp);
    }

    public void Dispose() => _exp.finish();

    [Fact]
    public void GetProbability_ReturnsConstructorValue()
    {
        var dist = new BoolDistBernoulli(_model, "Bern", 0.3, false, false);
        Assert.Equal(0.3, dist.getProbability(), precision: 9);
    }

    [Fact]
    public void AlwaysTrue_AllSamplesTrue()
    {
        var dist = new BoolDistBernoulli(_model, "BernTrue", 1.0, false, false);
        for (int i = 0; i < 20; i++)
            Assert.True(dist.sample());
    }

    [Fact]
    public void AlwaysFalse_AllSamplesFalse()
    {
        var dist = new BoolDistBernoulli(_model, "BernFalse", 0.0, false, false);
        for (int i = 0; i < 20; i++)
            Assert.False(dist.sample());
    }

    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var dist = new BoolDistBernoulli(_model, "BernSeed", 0.5, false, false);
        dist.setSeed(42L);
        var seq1 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();
        dist.setSeed(42L);
        var seq2 = Enumerable.Range(0, 10).Select(_ => dist.sample()).ToArray();
        Assert.Equal(seq1, seq2);
    }
}
