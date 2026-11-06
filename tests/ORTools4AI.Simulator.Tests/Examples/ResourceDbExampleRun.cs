// Modified for ORTools4AI.Simulator.


using System.Collections;
using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test
{
    public class ResourceDbExampleRun
    {
        [Fact(Timeout = 120_000)]
        public void MachineLineResourceDb_Runs()
        {
            Assert.NotEqual(3, MachineModel.Run());
        }
    }

    /// <summary>
    /// A simple M/M/c queueing model: workers arrive, seize one of 3 machines,
    /// hold it for service, then release it. Demonstrates ResourceDB via Res.
    /// </summary>
    public class MachineModel : Model
    {
        public Res? Machines;

        private ContDistExponential? _arrivalDist;
        private ContDistExponential? _serviceDist;

        public MachineModel(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        public override string description() =>
            "Simple machine-line model: workers arrive and seize one of 3 machines.";

        public override void init()
        {
            Machines = new Res(this, "Machines", 3, true, true);

            _arrivalDist = new ContDistExponential(this, "ArrivalTime", 5.0, true, false);
            _arrivalDist.setNonNegative(true);

            _serviceDist = new ContDistExponential(this, "ServiceTime", 8.0, true, false);
            _serviceDist.setNonNegative(true);
        }

        public override void doInitialSchedules()
        {
            var generator = new WorkerGenerator(this, "WorkerGenerator", false);
            generator.activate(new TimeSpan(0.0));
        }

        public double getArrivalTime() => _arrivalDist!.sample();
        public double getServiceTime() => _serviceDist!.sample();

        public static int Run()
        {
            var model = new MachineModel(null, "MachineLineModel", true, true);
            var exp = new Experiment("ResourceDbExampleExperiment", TimeUnit.SECONDS, TimeUnit.MINUTES, null);

            model.connectToExperiment(exp);

            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(480, TimeUnit.MINUTES));
            exp.tracePeriod(new TimeInstant(0), new TimeInstant(30, TimeUnit.MINUTES));
            exp.debugPeriod(new TimeInstant(0), new TimeInstant(10, TimeUnit.MINUTES));

            exp.start();
            int status = exp.getStatus();
            exp.report();
            exp.finish();
            return status;
        }
    }

    /// <summary>
    /// Generates a stream of Worker processes with exponential inter-arrival times.
    /// </summary>
    public class WorkerGenerator : SimProcess
    {
        public WorkerGenerator(Model owner, string name, bool showInTrace)
            : base(owner, name, showInTrace) { }

        protected override IEnumerable Run()
        {
            var model = (MachineModel)getModel();

            while (true)
            {
                var worker = new Worker(model, "Worker", true);
                worker.activateAfter(this);

                yield return Delay(model.getArrivalTime());
            }
        }
    }

    /// <summary>
    /// A worker that seizes one machine unit, uses it for a service duration,
    /// then releases it back to the pool.
    /// </summary>
    public class Worker : SimProcess
    {
        private readonly MachineModel _model;

        public Worker(Model owner, string name, bool showInTrace)
            : base(owner, name, showInTrace)
        {
            _model = (MachineModel)owner;
        }

        protected override IEnumerable Run()
        {
            sendTraceNote("Requesting a machine.");

            // provide() returns false when resources are unavailable;
            // activateFirst() in takeBack() will reschedule us; yield until acquired.
            while (!_model.Machines!.provide(1))
                yield return Suspend();

            sendTraceNote("Acquired machine, starting work.");

            yield return Delay(_model.getServiceTime());

            sendTraceNote("Work done, releasing machine.");
            _model.Machines.takeBack(1);
        }
    }
}
