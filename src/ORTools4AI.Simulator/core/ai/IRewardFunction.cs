// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.ai;

public interface IRewardFunction
{
    double Compute();
    double ComputeTerminal();
    void   Reset();
}

public sealed class CompositeReward : IRewardFunction
{
    private readonly (IRewardFunction fn, double weight)[] _terms;

    public CompositeReward(params (IRewardFunction fn, double weight)[] terms)
        => _terms = terms;

    public double Compute()
        => _terms.Sum(t => t.weight * t.fn.Compute());

    public double ComputeTerminal()
        => _terms.Sum(t => t.weight * t.fn.ComputeTerminal());

    public void Reset()
    {
        foreach (var (fn, _) in _terms) fn.Reset();
    }
}
