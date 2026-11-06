// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.extensions.supplychain.demand;

internal class TraceDrivenDemandPolicy : IDemandPolicy
{
    public NumericalDist<double> InterArrivalTimeDist { get; }
    public NumericalDist<double> OrderQuantityDist { get; }

    // trace entries as (interArrivalTime, quantity)
    internal TraceDrivenDemandPolicy(Model owner, List<(double interArrival, double qty)> trace,
        string name = "TraceDemand")
    {
        var interArrivals = new TraceReplayDist(owner, name + "_InterArrival",
            trace.Select(t => t.interArrival).ToList());
        var quantities = new TraceReplayDist(owner, name + "_Qty",
            trace.Select(t => t.qty).ToList());
        InterArrivalTimeDist = interArrivals;
        OrderQuantityDist = quantities;
    }
}

// Cycles through a fixed sequence of values, wrapping at the end
internal class TraceReplayDist : NumericalDist<double>
{
    private readonly List<double> _values;
    private int _index;

    internal TraceReplayDist(Model owner, string name, List<double> values)
        : base(owner, name, false, false)
    {
        _values = values.Count > 0 ? values : [1.0];
        _index = 0;
    }

    public override double getInverseOfCumulativeProbabilityFunction(double p)
    {
        double val = _values[_index % _values.Count];
        _index++;
        return val;
    }
}
