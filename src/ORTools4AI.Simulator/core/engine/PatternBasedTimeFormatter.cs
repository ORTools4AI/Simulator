// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{
    public class PatternBasedTimeFormatter : TimeFormatter
    {
        private readonly string _pattern;
        private readonly bool _includeTimeZoneId;
        private readonly MultiUnitTimeFormatter _spanFormatter;

        public PatternBasedTimeFormatter()
            : this(false) { }

        public PatternBasedTimeFormatter(bool timeZoneIncluded)
            : this("dd.MM.yyyy HH:mm:ss:fff", timeZoneIncluded, TimeUnit.DAYS, TimeUnit.MILLISECONDS) { }

        public PatternBasedTimeFormatter(string pattern, TimeUnit coarsestUnit, TimeUnit finestUnit)
            : this(pattern, false, coarsestUnit, finestUnit) { }

        private PatternBasedTimeFormatter(string pattern, bool includeTimeZoneId, TimeUnit coarsestUnit, TimeUnit finestUnit)
        {
            _pattern = pattern;
            _includeTimeZoneId = includeTimeZoneId;
            _spanFormatter = new MultiUnitTimeFormatter(coarsestUnit, finestUnit);
        }

        public string buildTimeString(TimeInstant instant)
        {
            long ms = instant.getTimeTruncated(TimeUnit.MILLISECONDS);
            TimeZoneInfo tz = instant.getPreferredTimeZone();
            DateTimeOffset dto = TimeZoneInfo.ConvertTime(
                DateTimeOffset.FromUnixTimeMilliseconds(ms), tz);
            string formatted = dto.ToString(_pattern);
            if (_includeTimeZoneId)
                formatted += " " + tz.Id;
            return formatted;
        }

        public string buildTimeString(TimeSpan span) =>
            _spanFormatter.buildTimeString(span);

        public string getUnit() => _spanFormatter.getUnit();
    }
}
