// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine;

/// <summary>
/// Logs every EventNote dequeued by the Scheduler to a plain-text pipe-delimited file.
/// Extends FileOutput so Experiment.registerFileOutput() automatically calls close() on finish().
/// </summary>
public class EventListLogger : FileOutput
{
    private long _sequenceNumber;

    private const string Sep     = "|";
    private const string NullVal = "NULL";

    /// <summary>
    /// Opens the log file and writes the header block.
    /// Call this after connectToExperiment() and before exp.start().
    /// </summary>
    public void open(Experiment experiment, string? filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = experiment.getName() + "_eventlist.txt";

        base.open(filePath);

        _sequenceNumber = 0;

        writeln("# DESMOJ EventList Log");
        writeln("# ExperimentName: " + experiment.getName());
        writeln("# ReferenceUnit: "  + TimeOperations.getReferenceUnit().ToString());
        writeln("# Seed: "           + experiment.getDistributionManager().getSeed());
        writeln("# StatusAtStart: "  + experiment.getStatus());
        writeln("# Date: "           + DateTime.UtcNow.ToString("o"));
        writeln("# Columns: SeqNo|SimTime|Priority|EventType|Entity1|Entity2|Entity3|Source|NoteType");
    }

    /// <summary>
    /// Logs one EventNote. Called from Scheduler.processNextEventNote() right after removeFirst().
    /// No-op if the file is not open or note is null.
    /// </summary>
    public void logNote(EventNote? note)
    {
        if (!fileOpen || note == null) return;

        _sequenceNumber++;

        string simTime   = note.getTime().getTimeInEpsilon().ToString();
        string priority  = note.getPriority().ToString();
        string eventType = note.getEvent() != null
                               ? Escape(note.getEvent()!.GetType().FullName ?? NullVal)
                               : NullVal;
        string entity1   = note.getEntity1() != null
                               ? Escape(note.getEntity1()!.getName())
                               : NullVal;
        string entity2   = note.getEntity2() != null
                               ? Escape(note.getEntity2()!.getName())
                               : NullVal;
        string entity3   = note.getEntity3() != null
                               ? Escape(note.getEntity3()!.getName())
                               : NullVal;
        string source    = note.getSource() != null
                               ? Escape(note.getSource()!.getName())
                               : NullVal;
        string noteType  = note.getEvent() == null ? "SIMPROCESS" : "EVENT";

        writeln(string.Join(Sep,
            _sequenceNumber, simTime, priority,
            eventType, entity1, entity2, entity3,
            source, noteType));
    }

    // close() is fully inherited from FileOutput.

    private static string Escape(string? name)
        => name == null ? NullVal : name.Replace("|", @"\|");
}
