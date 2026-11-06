// Modified for ORTools4AI.Simulator.

using System.Text.RegularExpressions;

namespace ORTools4AI.Simulator.core.engine;

/// <summary>
/// Typed record for one data line from an EventList log.
/// Name fields contain "NULL" when the original value was null.
/// </summary>
public sealed record EventLogRecord(
    long   SeqNo,
    long   SimTimeInEpsilon,
    int    Priority,
    string EventType,
    string Entity1Name,
    string Entity2Name,
    string Entity3Name,
    string SourceName,
    string NoteType);

/// <summary>
/// Reads an EventList log file back into typed records.
/// Use for comparison runs, regression tests, and replay validation.
/// </summary>
public class EventListLogReader
{
    public string ExperimentName  { get; private set; } = "";
    public string ReferenceUnit   { get; private set; } = "";
    public long   Seed            { get; private set; }
    public int    StatusAtStart   { get; private set; }
    public string Date            { get; private set; } = "";

    // Splits on '|' not preceded by '\'
    private static readonly Regex SplitPipe =
        new Regex(@"(?<!\\)\|", RegexOptions.Compiled);

    /// <summary>
    /// Parses the log file. Header metadata is available as properties after this call.
    /// </summary>
    /// <exception cref="IOException">File cannot be read.</exception>
    /// <exception cref="FormatException">A data line is malformed.</exception>
    public List<EventLogRecord> ReadLog(string filePath)
    {
        var records = new List<EventLogRecord>();

        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("#"))
            {
                ParseHeader(line);
                continue;
            }

            records.Add(ParseData(line));
        }

        return records;
    }

    private void ParseHeader(string line)
    {
        int colon = line.IndexOf(':');
        if (colon < 0) return;
        string key   = line[1..colon].Trim();
        string value = line[(colon + 1)..].Trim();
        switch (key)
        {
            case "ExperimentName": ExperimentName = value; break;
            case "ReferenceUnit":  ReferenceUnit  = value; break;
            case "Seed":           Seed           = long.Parse(value); break;
            case "StatusAtStart":  StatusAtStart  = int.Parse(value); break;
            case "Date":           Date           = value; break;
        }
    }

    private static EventLogRecord ParseData(string line)
    {
        string[] parts = SplitPipe.Split(line);
        if (parts.Length != 9)
            throw new FormatException(
                $"Expected 9 pipe-separated columns, got {parts.Length}: '{line}'");

        return new EventLogRecord(
            SeqNo:            long.Parse(parts[0]),
            SimTimeInEpsilon: long.Parse(parts[1]),
            Priority:         int.Parse(parts[2]),
            EventType:        Unescape(parts[3]),
            Entity1Name:      Unescape(parts[4]),
            Entity2Name:      Unescape(parts[5]),
            Entity3Name:      Unescape(parts[6]),
            SourceName:       Unescape(parts[7]),
            NoteType:         parts[8]);
    }

    private static string Unescape(string s) => s.Replace(@"\|", "|");
}
