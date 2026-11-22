using ORTools4AI.Simulator.core.engine;
using SimulationDuration = ORTools4AI.Simulator.core.engine.TimeSpan;

var experiment = new Experiment("Package smoke test", output: false);
var model = new SmokeTestModel();
model.connectToExperiment(experiment);
experiment.stop(new TimeInstant(4, TimeUnit.SECONDS));

try
{
    experiment.start();
}
finally
{
    experiment.finish();
}

double[] expectedTimes = [1, 3];
if (!model.EventTimes.SequenceEqual(expectedTimes))
{
    Console.Error.WriteLine($"Expected events at 1 and 3 seconds. Actual: {string.Join(", ", model.EventTimes)}");
    return 1;
}

Console.WriteLine("Package smoke test passed. Events ran at 1 and 3 simulation seconds.");
return 0;

sealed class SmokeTestModel() : Model(null!, "Event ordering", showInReport: false, showInTrace: false)
{
    public List<double> EventTimes { get; } = [];

    public override string description() => "Verifies that the simulator runs events in time order.";

    public override void init() { }

    public override void doInitialSchedules()
    {
        new RecordTimeEvent(this, "Later event").schedule(new SimulationDuration(3, TimeUnit.SECONDS));
        new RecordTimeEvent(this, "Earlier event").schedule(new SimulationDuration(1, TimeUnit.SECONDS));
    }

    private sealed class RecordTimeEvent(SmokeTestModel owner, string name)
        : ExternalEvent(owner, name, showInTrace: false)
    {
        public override void eventRoutine() =>
            owner.EventTimes.Add(presentTime().getTimeAsDouble(TimeUnit.SECONDS));
    }
}
