// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Generates parts at a configurable inter-arrival time distribution,
// optionally delays each part by a travel-time distribution before delivery.
internal class ConveyorProcess : SimProcess
{
    private readonly NumericalDist<double>  _iatDist;
    private readonly NumericalDist<double>? _travelTimeDist;
    private readonly Func<string>           _partTypeSelector;
    private readonly Action<Part>           _onArrival;

    internal ConveyorProcess(
        Model owner,
        string conveyorId,
        NumericalDist<double>  iatDist,
        Func<string>           partTypeSelector,
        Action<Part>           onArrival,
        NumericalDist<double>? travelTimeDist = null,
        bool showInTrace = false)
        : base(owner, $"Conveyor-{conveyorId}", showInTrace)
    {
        _iatDist          = iatDist;
        _travelTimeDist   = travelTimeDist;
        _partTypeSelector = partTypeSelector;
        _onArrival        = onArrival;
    }

    protected override IEnumerable Run()
    {
        while (true)
        {
            yield return Delay(_iatDist.sample());

            var part = new Part(getModel(), _partTypeSelector(), presentTime());

            if (_travelTimeDist is not null)
            {
                yield return Delay(_travelTimeDist.sample());
            }

            _onArrival(part);
        }
    }
}
