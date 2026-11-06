// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test
{
    public class ChainingExampleRun
    {
        [Fact(Timeout = 120_000)]
        public void ChainingProductionLine_Runs()
        {
            Assert.NotEqual(3, ChainingExample.Run());
        }
    }

    // ── Entities ────────────────────────────────────────────────────────────

    public class RawPart : Entity
    {
        public RawPart() : base(null!, "RawPart", false) { }
        public RawPart(Model model) : base(model, "RawPart", true) { }
    }

    public class FinishedPart : Entity
    {
        public FinishedPart(Model model) : base(model, "FinishedPart", true) { }
    }

    // ── Stations ─────────────────────────────────────────────────────────────

    // Source: endlessly produces RawPart entities one at a time.
    public class PartSource : Source<RawPart>
    {
        public PartSource(Model owner, NumericalDist<double> productionTime)
            : base(
                maxEntitiesToProduce: 20,
                parallelProducedEntities: 1,
                setupTime: null,
                productionTime: productionTime,
                recoveryTime: null,
                transportTime: null,
                owner: owner,
                name: "PartSource",
                showInReport: true,
                showInTrace: true)
        { }

        protected override RawPart createEntity() => new RawPart(getModel());
    }

    // Transformer: processes a RawPart and emits a FinishedPart.
    public class AssemblyStation : Transformer<RawPart, FinishedPart>
    {
        public AssemblyStation(Model owner, NumericalDist<double> serviceTime)
            : base(
                incomingBufferQueueCapacity: 10,
                maxEntitiesToHandle: -1,
                parallelHandledEntities: 1,
                setupTime: null,
                serviceTime: serviceTime,
                recoveryTime: null,
                transportTime: null,
                owner: owner,
                name: "AssemblyStation",
                showInReport: true,
                showInTrace: true)
        { }

        protected override FinishedPart transformEntity(RawPart originalEntity)
            => new FinishedPart(getModel());
    }

    // Sink: receives FinishedPart entities — terminal node, no successor.
    public class FinishedGoodsSink : Sink<FinishedPart>
    {
        public FinishedGoodsSink(Model owner)
            : base(
                incomingBufferQueueCapacity: -1,
                maxEntitiesToHandle: -1,
                parallelHandledEntities: 1,
                setupTime: null,
                serviceTime: null,
                recoveryTime: null,
                owner: owner,
                name: "FinishedGoodsSink",
                showInReport: true,
                showInTrace: true)
        { }
    }

    // ── Model ─────────────────────────────────────────────────────────────────

    public class ChainingExample : Model
    {
        private PartSource? _source;
        private AssemblyStation? _assembly;
        private FinishedGoodsSink? _sink;

        public ChainingExample(Model? owner, string name)
            : base(owner, name, true, true) { }

        public override string description() =>
            "Simple chaining example: Source → AssemblyStation → Sink";

        public override void init()
        {
            var productionTime = new ContDistExponential(this, "ProductionTime", 2.0, true, false);
            var serviceTime    = new ContDistExponential(this, "ServiceTime",    3.0, true, false);

            _source   = new PartSource(this, productionTime);
            _assembly = new AssemblyStation(this, serviceTime);
            _sink     = new FinishedGoodsSink(this);

            // Wire: source → assembly → sink
            _source.setSuccessor(_assembly);
            _assembly.setSuccessor(_sink);
        }

        public override void doInitialSchedules()
        {
            _source!.startCreatingEntities();
        }

        public static int Run()
        {
            var model = new ChainingExample(null, "ProductionLine");
            var exp   = new Experiment("ChainingExampleExperiment", TimeUnit.SECONDS, TimeUnit.MINUTES, null);

            model.connectToExperiment(exp);

            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(100, TimeUnit.MINUTES));
            exp.tracePeriod(new TimeInstant(0), new TimeInstant(20, TimeUnit.MINUTES));
            exp.debugPeriod(new TimeInstant(0), new TimeInstant(10, TimeUnit.MINUTES));

            exp.start();
            int status = exp.getStatus();
            exp.report();
            exp.finish();
            return status;
        }
    }
}
