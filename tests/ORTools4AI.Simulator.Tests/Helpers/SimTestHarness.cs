// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.Tests.Helpers;

internal class HarnessModel : Model
{
    public HarnessModel() : base(null, "HarnessModel", false, false) { }
    public override string description() => "Test";
    public override void init() { }
    public override void doInitialSchedules() { }
}

internal static class SimTestHarness
{
    public static (HarnessModel model, Experiment exp) Create(string? name = null)
    {
        var exp = new Experiment(name ?? $"TestExp-{Guid.NewGuid():N}", false);
        var model = new HarnessModel();
        model.connectToExperiment(exp);
        return (model, exp);
    }
}
