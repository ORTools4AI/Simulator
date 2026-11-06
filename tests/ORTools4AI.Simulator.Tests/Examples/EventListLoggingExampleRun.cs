// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;

using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace SlogTek.Desmoj.Test
{
    public class EventListLoggingExampleRun
    {
        private static string TempFile(string name) =>
            Path.Combine(Path.GetTempPath(), name);

        // ── Test 1: log is created, parseable, and structurally valid ──────────

        [Fact(Timeout = 120_000)]
        public void EventListLogging_WritesReadableLog()
        {
            string logPath = TempFile("desmoj_log_write_test.txt");
            try
            {
                Assert.NotEqual(3, LoggingTestRig.Run(logPath, seed: null));

                Assert.True(File.Exists(logPath), "Log file was not created.");

                var reader  = new EventListLogReader();
                var records = reader.ReadLog(logPath);

                Assert.NotEmpty(records);

                // SeqNos are 1-based and monotonically increasing
                Assert.Equal(1L, records[0].SeqNo);
                for (int i = 1; i < records.Count; i++)
                    Assert.Equal(records[i - 1].SeqNo + 1, records[i].SeqNo);

                // SimTime is non-decreasing
                for (int i = 1; i < records.Count; i++)
                    Assert.True(
                        records[i].SimTimeInEpsilon >= records[i - 1].SimTimeInEpsilon,
                        $"SimTime went backwards at record {records[i].SeqNo}");

                // Header seed is positive
                Assert.True(reader.Seed > 0, "Seed missing from log header.");

                // Status at the point logging was enabled should be CONNECTED (−1)
                // enableEventListLogging() is called after connectToExperiment() and before start()
                Assert.Equal(Experiment.CONNECTED, reader.StatusAtStart);
            }
            finally
            {
                if (File.Exists(logPath)) File.Delete(logPath);
            }
        }

        // ── Test 2: re-run with same seed produces bit-for-bit identical log ───

        [Fact(Timeout = 120_000)]
        public void EventListLogging_ReproductionRunProducesIdenticalLog()
        {
            string log1 = TempFile("desmoj_repro_run1.txt");
            string log2 = TempFile("desmoj_repro_run2.txt");
            try
            {
                // Run 1 — original
                Assert.NotEqual(3, LoggingTestRig.Run(log1, seed: null));

                // Read seed from run 1
                var reader1  = new EventListLogReader();
                var records1 = reader1.ReadLog(log1);

                // Run 2 — reproduce using the logged seed
                Assert.NotEqual(3, LoggingTestRig.Run(log2, seed: reader1.Seed));

                var reader2  = new EventListLogReader();
                var records2 = reader2.ReadLog(log2);

                Assert.Equal(records1.Count, records2.Count);
                Assert.True(
                    records1.SequenceEqual(records2),
                    "Re-run with the same seed did not produce the identical event sequence.");
            }
            finally
            {
                if (File.Exists(log1)) File.Delete(log1);
                if (File.Exists(log2)) File.Delete(log2);
            }
        }

        // ── Test 3: SimProcess example with logging ────────────────────────────

        [Fact(Timeout = 120_000)]
        public void SimProcessExample_ProducesNonEmptyLog()
        {
            string logPath = TempFile("desmoj_simprocess_log.txt");
            try
            {
                var model = new SimProcessLoggingWrapper(null, "VanCarrierModel", false, false);
                var exp   = new Experiment("SimProcessLoggingExp", TimeUnit.SECONDS, TimeUnit.MINUTES, null);
                model.connectToExperiment(exp);
                exp.setShowProgressBar(false);
                exp.stop(new TimeInstant(60, TimeUnit.MINUTES));

                exp.enableEventListLogging(logPath);
                exp.start();
                Assert.NotEqual(3, exp.getStatus());
                exp.finish();

                var records = new EventListLogReader().ReadLog(logPath);
                Assert.NotEmpty(records);
                Assert.True(records.Any(r => r.NoteType == "SIMPROCESS"),
                    "Expected at least one SIMPROCESS note.");
            }
            finally
            {
                if (File.Exists(logPath)) File.Delete(logPath);
            }
        }

        // ── Test 4: ResourceDB (Res) example with logging ─────────────────────

        [Fact(Timeout = 120_000)]
        public void ResourceDbExample_ProducesNonEmptyLog()
        {
            string logPath = TempFile("desmoj_resdb_log.txt");
            try
            {
                var model = new MachineModelLoggingWrapper(null, "MachineModel", false, false);
                var exp   = new Experiment("MachineModelLoggingExp", TimeUnit.SECONDS, TimeUnit.MINUTES, null);
                model.connectToExperiment(exp);
                exp.setShowProgressBar(false);
                exp.stop(new TimeInstant(60, TimeUnit.MINUTES));

                exp.enableEventListLogging(logPath);
                exp.start();
                Assert.NotEqual(3, exp.getStatus());
                exp.finish();

                var records = new EventListLogReader().ReadLog(logPath);
                Assert.NotEmpty(records);
            }
            finally
            {
                if (File.Exists(logPath)) File.Delete(logPath);
            }
        }
    }

    // ── Minimal deterministic rig used by Tests 1 & 2 ────────────────────────

    internal static class LoggingTestRig
    {
        internal static int Run(string logPath, long? seed)
        {
            var model = new LoggingTestModel(null, "LoggingTest", false, false);
            var exp   = new Experiment("LoggingTestExp", TimeUnit.SECONDS, TimeUnit.MINUTES, null);
            model.connectToExperiment(exp);
            exp.setShowProgressBar(false);
            exp.stop(new TimeInstant(50, TimeUnit.MINUTES));

            if (seed.HasValue)
                exp.setSeedGenerator(seed.Value);

            exp.enableEventListLogging(logPath);
            exp.start();
            int status = exp.getStatus();
            exp.finish();
            return status;
        }
    }

    internal class LoggingTestModel : Model
    {
        private ContDistExponential? _arrivalDist;
        private ContDistExponential? _serviceDist;

        public LoggingTestModel(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }

        public override string description() => "Minimal model for EventList logging tests.";

        public override void init()
        {
            _arrivalDist = new ContDistExponential(this, "Arrival", 3.0, false, false);
            _arrivalDist.setNonNegative(true);
            _serviceDist = new ContDistExponential(this, "Service", 5.0, false, false);
            _serviceDist.setNonNegative(true);
        }

        public override void doInitialSchedules()
        {
            new LoggingTestGenerator(this, "Generator", false).activate(new TimeSpan(0.0));
        }

        public double nextArrival() => _arrivalDist!.sample();
        public double nextService() => _serviceDist!.sample();
    }

    internal class LoggingTestGenerator : SimProcess
    {
        public LoggingTestGenerator(Model owner, string name, bool showInTrace)
            : base(owner, name, showInTrace) { }

        protected override IEnumerable Run()
        {
            var m = (LoggingTestModel)getModel();
            while (true)
            {
                new LoggingTestWorker(m, "Worker", false).activate(new TimeSpan(0.0));
                yield return Delay(m.nextArrival());
            }
        }
    }

    internal class LoggingTestWorker : SimProcess
    {
        public LoggingTestWorker(Model owner, string name, bool showInTrace)
            : base(owner, name, showInTrace) { }

        protected override IEnumerable Run()
        {
            var m = (LoggingTestModel)getModel();
            yield return Delay(m.nextService());
        }
    }

    // ── SimProcess Van Carrier model wired for Test 3 ─────────────────────────

    internal class SimProcessLoggingWrapper : SimProcessExample
    {
        public SimProcessLoggingWrapper(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }
    }

    // ── ResourceDB Machine model wired for Test 4 ─────────────────────────────

    internal class MachineModelLoggingWrapper : MachineModel
    {
        public MachineModelLoggingWrapper(Model owner, string name, bool showInReport, bool showInTrace)
            : base(owner, name, showInReport, showInTrace) { }
    }
}
