// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Python;

/// <summary>
/// Concrete Entity with a string-keyed property bag for dynamic attributes.
///
/// Use when you need a lightweight entity whose attributes are set at runtime
/// from Python rather than in a dedicated C# subclass.
///
/// Python example:
/// <code>
/// customer = PythonEntity(model, "Customer", True)
/// customer.Set("priority",    1)
/// customer.Set("arrivalTime", model.presentTime())
///
/// prio    = customer.Get("priority")     # returns object or None
/// arrived = customer.Has("arrivalTime")  # True / False
/// customer.Remove("priority")
/// </code>
/// </summary>
public sealed class PythonEntity : Entity
{
    private readonly Dictionary<string, object?> _properties = new();

    public PythonEntity(Model owner, string name, bool showInTrace)
        : base(owner, name, showInTrace) { }

    /// <summary>Sets or overwrites a named property.</summary>
    public void Set(string key, object? value) => _properties[key] = value;

    /// <summary>Returns the property value, or <c>null</c> if not set.</summary>
    public object? Get(string key) =>
        _properties.TryGetValue(key, out var v) ? v : null;

    /// <summary>Returns <c>true</c> if the property exists.</summary>
    public bool Has(string key) => _properties.ContainsKey(key);

    /// <summary>Removes the property; no-op if it does not exist.</summary>
    public void Remove(string key) => _properties.Remove(key);

    /// <summary>Read-only view of all properties.</summary>
    public IReadOnlyDictionary<string, object?> Properties => _properties;
}
