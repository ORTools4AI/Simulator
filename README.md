# ORTools4AI.Simulator

ORTools4AI.Simulator is a .NET 8 discrete-event simulation library based on a C# port of [DESMO-J](https://desmoj.sourceforge.net/home.html). It supports event- and process-oriented models, simulation clocks and scheduling, queues and resources, random distributions, and statistics.

## Build and test

With the .NET 8 SDK installed, open `ORTools4AI.sln` in Visual Studio or run:

```sh
dotnet build ORTools4AI.sln
dotnet test ORTools4AI.sln --no-build
```

The solution contains the simulation library, the C# bridge for Python integration, and the .NET test suite.

## Origin and credits

The DESMO-J project and its contributors developed the original Java framework. ORTools4AI.Simulator packages the C# port, Python bridge, and simulator extensions in a standalone solution with updated namespaces and C# documentation.

See [NOTICE](NOTICE) for source provenance and [LICENSE](LICENSE) for the Apache 2.0 terms covering the DESMO-J-derived code. File headers retain upstream credits where applicable.
