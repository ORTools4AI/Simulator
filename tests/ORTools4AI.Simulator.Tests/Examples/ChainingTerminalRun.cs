// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test
{
    public class ChainingTerminalRun
    {
        [Fact(Timeout = 120_000)]
        public void ChainingTerminalSimulation_Runs()
        {
            Assert.NotEqual(3, ChainingTerminalExample.Run());
        }
    }

    // ── Entity ───────────────────────────────────────────────────────────────

    // Parameterless constructor required by Source<E>'s new() constraint.
    public class TerminalTruck : Entity
    {
        public TerminalTruck() : base(null!, "Truck", false) { }
        public TerminalTruck(Model model) : base(model, "Truck", true) { }
    }

    // ── Stations ─────────────────────────────────────────────────────────────

    // Source: each "service cycle" represents one inter-arrival period.
    // When the cycle completes it creates a real TerminalTruck and forwards it
    // to the VanCarrierStation, then immediately queues a new dummy to keep
    // generating arrivals.
    public class TruckArrivalSource : Source<TerminalTruck>
    {
        public TruckArrivalSource(Model owner, NumericalDist<double> interArrivalTime)
            : base(
                maxEntitiesToProduce: -1,           // unlimited arrivals
                parallelProducedEntities: 1,        // one truck at a time
                setupTime: null,
                productionTime: interArrivalTime,   // time between arrivals
                recoveryTime: null,
                transportTime: null,
                owner: owner,
                name: "TruckArrivalSource",
                showInReport: true,
                showInTrace: true)
        { }

        protected override TerminalTruck createEntity() => new TerminalTruck(getModel());
    }

    // Station: represents the loading area staffed by NUM_VC van carriers.
    // parallelHandledEntities = NUM_VC gives each VC its own capacity token.
    // Trucks queue in incomingBufferQueue (parking lot) until a VC is free,
    // then get serviced and forwarded to the departure sink.
    public class VanCarrierStation : ServiceStation<TerminalTruck>
    {
        public VanCarrierStation(Model owner, int numVanCarriers, NumericalDist<double> serviceTime)
            : base(
                incomingBufferQueueCapacity: -1,    // unlimited parking lot
                maxEntitiesToHandle: -1,
                parallelHandledEntities: numVanCarriers,
                setupTime: null,
                serviceTime: serviceTime,
                recoveryTime: null,
                transportTime: null,
                owner: owner,
                name: "VanCarrierStation",
                showInReport: true,
                showInTrace: true)
        { }
    }

    // Sink: terminal node representing trucks leaving the terminal after loading.
    public class TruckDepartureSink : Sink<TerminalTruck>
    {
        public TruckDepartureSink(Model owner)
            : base(
                incomingBufferQueueCapacity: -1,
                maxEntitiesToHandle: -1,
                parallelHandledEntities: 1,
                setupTime: null,
                serviceTime: null,
                recoveryTime: null,
                owner: owner,
                name: "TruckDepartureSink",
                showInReport: true,
                showInTrace: true)
        { }
    }

    // ── Model ─────────────────────────────────────────────────────────────────

    public class ChainingTerminalExample : Model
    {
        public static readonly int NUM_VC = 2;

        private TruckArrivalSource?  _source;
        private VanCarrierStation?   _station;
        private TruckDepartureSink?  _sink;

        public ChainingTerminalExample(Model? owner, string name)
            : base(owner, name, true, true) { }

        public override string description() =>
            "Container terminal via chaining: " +
            "TruckArrivalSource → VanCarrierStation (parallelHandledEntities = NUM_VC) → TruckDepartureSink";

        public override void init()
        {
            var interArrivalTime = new ContDistExponential(
                this, "TruckArrivalTimeStream", 3.0, true, false);
            interArrivalTime.setNonNegative(true);

            var serviceTime = new ContDistUniform(
                this, "ServiceTimeStream", 3.0, 7.0, true, false);

            _source  = new TruckArrivalSource(this, interArrivalTime);
            _station = new VanCarrierStation(this, NUM_VC, serviceTime);
            _sink    = new TruckDepartureSink(this);

            // Wire: Source → VanCarrierStation → Sink
            _source.setSuccessor(_station);
            _station.setSuccessor(_sink);
        }

        public override void doInitialSchedules()
        {
            _source!.startCreatingEntities();
        }

        public static int Run()
        {
            var model = new ChainingTerminalExample(null, "ChainingTerminal");
            var exp   = new Experiment(
                "ChainingTerminalExperiment", TimeUnit.SECONDS, TimeUnit.MINUTES, null);

            model.connectToExperiment(exp);

            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(1500, TimeUnit.MINUTES));
            exp.tracePeriod(new TimeInstant(0), new TimeInstant(100, TimeUnit.MINUTES));
            exp.debugPeriod(new TimeInstant(0), new TimeInstant(50,  TimeUnit.MINUTES));

            exp.start();
            int status = exp.getStatus();
            exp.report();
            exp.finish();
            return status;
        }
    }
}
