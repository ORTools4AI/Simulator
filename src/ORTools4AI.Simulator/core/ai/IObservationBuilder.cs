// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.ai;

public interface IObservationBuilder<TObs>
{
    TObs Build();
}

public class VectorObservation(float[] features)
{
    public float[] Features { get; } = features;
    public int Size => features.Length;
}

public abstract class VectorObservationBuilder(Model model)
    : IObservationBuilder<VectorObservation>
{
    protected Model Model { get; } = model;

    public VectorObservation Build() => new(BuildFeatures());

    protected abstract float[] BuildFeatures();
}
