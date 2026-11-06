// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.production;

// Stateless classifier: routes a part to one of several downstream handlers
// based on its PartType. Routing is instantaneous — no SimProcess needed.
internal sealed class PartRouter
{
    private readonly Func<Part, string>              _classify;
    private readonly Dictionary<string, Action<Part>> _routes;
    private readonly Action<Part>?                   _defaultRoute;

    internal PartRouter(
        Func<Part, string>               classify,
        Dictionary<string, Action<Part>> routes,
        Action<Part>?                    defaultRoute = null)
    {
        _classify     = classify;
        _routes       = routes;
        _defaultRoute = defaultRoute;
    }

    internal void Route(Part part)
    {
        var key = _classify(part);
        if (_routes.TryGetValue(key, out var handler))
            handler(part);
        else
            _defaultRoute?.Invoke(part);
    }
}
