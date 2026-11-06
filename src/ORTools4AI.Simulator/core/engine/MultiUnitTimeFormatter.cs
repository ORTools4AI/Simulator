// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{
    public class MultiUnitTimeFormatter : TimeFormatter
    {
        private readonly List<TimeUnit> _unitList;
        private readonly char _separator;
        private readonly string _unit;

        private static readonly Dictionary<TimeUnit, int> numberOfDigits = new Dictionary<TimeUnit, int>
        {
            { TimeUnit.HOURS,        2 },
            { TimeUnit.MINUTES,      2 },
            { TimeUnit.SECONDS,      2 },
            { TimeUnit.MILLISECONDS, 3 },
            { TimeUnit.MICROSECONDS, 3 },
            { TimeUnit.NANOSECONDS,  3 },
        };

        public MultiUnitTimeFormatter()
            : this(TimeUnit.MILLISECONDS, TimeUnit.HOURS, ':') { }

        public MultiUnitTimeFormatter(TimeUnit coarsestUnit, TimeUnit finestUnit)
            : this(coarsestUnit, finestUnit, ':') { }

        public MultiUnitTimeFormatter(TimeUnit coarsestUnit, TimeUnit finestUnit, char separator)
            : this(coarsestUnit, finestUnit, separator, null) { }

        public MultiUnitTimeFormatter(TimeUnit coarsestUnit, TimeUnit finestUnit, char separator, string? unitString)
        {
            if (coarsestUnit.CompareTo(finestUnit) < 0)
            {
                TimeUnit tmp = coarsestUnit;
                coarsestUnit = finestUnit;
                finestUnit = tmp;
            }

            // build ordered list from finest to coarsest
            var all = new List<TimeUnit>
            {
                TimeUnit.NANOSECONDS, TimeUnit.MICROSECONDS, TimeUnit.MILLISECONDS,
                TimeUnit.SECONDS, TimeUnit.MINUTES, TimeUnit.HOURS, TimeUnit.DAYS
            };
            _unitList = all
                .Where(u => u.CompareTo(finestUnit) >= 0 && u.CompareTo(coarsestUnit) <= 0)
                .ToList();

            _separator = separator;

            if (unitString != null)
            {
                _unit = unitString;
            }
            else
            {
                var buf = new System.Text.StringBuilder();
                bool first = true;
                foreach (TimeUnit u in _unitList)
                {
                    if (!first)
                        buf.Insert(0, separator);
                    else
                        first = false;

                    if (u == TimeUnit.DAYS)
                    {
                        char c = u.ToString().ToLower()[0];
                        buf.Insert(0, c);
                        buf.Insert(0, c);
                    }
                    else
                    {
                        int digits = numberOfDigits[u];
                        char c = u.ToString().ToLower()[0];
                        for (int i = 0; i < digits; i++)
                            buf.Insert(0, c);
                    }
                }
                _unit = buf.ToString();
            }
        }

        public string buildTimeString(TimeInstant instant) =>
            buildMultiUnitTimeString(instant.getTimeInEpsilon());

        public string buildTimeString(TimeSpan span) =>
            buildMultiUnitTimeString(span.getTimeInEpsilon());

        public string getUnit() => _unit;

        private string buildMultiUnitTimeString(long timeValue)
        {
            TimeUnit eps = TimeOperations.getEpsilon();
            var buf = new System.Text.StringBuilder();

            for (int i = 0; i < _unitList.Count; i++)
            {
                TimeUnit currentUnit = _unitList[i];
                long timeCurrentUnit = currentUnit.Convert(timeValue, eps);

                if (i < _unitList.Count - 1)
                {
                    TimeUnit biggerUnit = _unitList[i + 1];
                    long aBiggerUnitAsCurrentUnit = currentUnit.Convert(1, biggerUnit);
                    timeCurrentUnit = timeCurrentUnit % aBiggerUnitAsCurrentUnit;

                    var unitBuf = new System.Text.StringBuilder();
                    unitBuf.Append(_separator);
                    string valStr = timeCurrentUnit.ToString();
                    int digits = (currentUnit == TimeUnit.DAYS) ? 2
                        : numberOfDigits.TryGetValue(currentUnit, out int d) ? d : 2;
                    for (int k = valStr.Length; k < digits; k++)
                        unitBuf.Append('0');
                    unitBuf.Append(valStr);
                    buf.Insert(0, unitBuf);
                }
                else
                {
                    buf.Insert(0, timeCurrentUnit);
                }
            }

            return buf.ToString();
        }
    }
}
