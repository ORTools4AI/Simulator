# ORTools4AI.Simulator

A .NET 8 discrete-event simulation library for event- and process-oriented models. The library includes event scheduling, simulation clocks, queues and resources, random distributions, and statistics.

## Quick start

After adding the package to a .NET 8 console project, try this simulation-time calculation:

```csharp
using ORTools4AI.Simulator.core.engine;
using SimulationDuration = ORTools4AI.Simulator.core.engine.TimeSpan;

var end = TimeOperations.add(new TimeInstant(2), new SimulationDuration(3));
Console.WriteLine(end.getTimeAsDouble()); // 5
```

## Licensing

This is a prerelease package. Tony Le offers his covered original contributions in core/ai and extensions under PolyForm Noncommercial 1.0.0. Inherited DESMO-J material retains its Apache 2.0 rights. See PACKAGE-LICENSE.md, the license texts, and NOTICE included in the package for scope and terms. For commercial licensing, contact tunglevn@gmail.com.
