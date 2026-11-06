// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author christian.mueller@th-wildau.de
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using System;
using System.Text;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.scheduling
{
    /**
     * EntityScheduleEntry is an entry of EntitySchedule.
     * A EntityScheduleEntry is a time region (e.g. every working day from 8:00 until 16:00).
     * All EntityScheduleEntry objects of an EntitySchedule share the same timezone, set by EntitySchedule.
     */
    public class EntityScheduleEntry
    {
        private TimeInstant? _validFrom;
        private TimeInstant? _validUntil;
        private bool[] _dayOfWeek;
        private readonly TimeSpan _fromBeginOfDay;
        private readonly TimeSpan _duration;
        private TimeZoneInfo _timezone;
        private object[] _values;

        /// <summary>
        /// Creates a schedule entry with a daily time window and an associated value. Defaults:
        /// validFrom/validUntil are null (unlimited); active on MON–FRI.
        /// </summary>
        /// <param name="model">the owning model (used for error reporting)</param>
        /// <param name="fromBeginOfDay">
        /// time from 00:00 to the start of the window (must be &lt; 24 h, not null)
        /// </param>
        /// <param name="untilBeginOfDay">time from 00:00 to the end of the window (not null)</param>
        /// <param name="value">payload value stored with this entry (not null)</param>
        public EntityScheduleEntry(Model model, TimeSpan fromBeginOfDay, TimeSpan untilBeginOfDay, object value)
        {
            if (fromBeginOfDay == null)
                throw new core.exception.SimAbortedException(
                    new core.report.ErrorMessage(null,
                        "fromBeginOfDay is null! Simulation aborted.",
                        "Class: EntityScheduleEntry  Constructor: fromBeginOfDay is null!",
                        "fromBeginOfDay is null!", "fromBeginOfDay is null!", null));

            if (TimeSpan.isLongerOrEqual(fromBeginOfDay, new TimeSpan(24, TimeUnit.HOURS)))
                throw new core.exception.SimAbortedException(
                    new core.report.ErrorMessage(null,
                        "fromBeginOfDay is longer than 24 hours! Simulation aborted.",
                        "Class: EntityScheduleEntry  Constructor: fromBeginOfDay >= 24 hours!",
                        "fromBeginOfDay is " + fromBeginOfDay.getTimeAsDouble(TimeUnit.HOURS) + " hours",
                        "fromBeginOfDay must be less than 24 hours.", null));

            if (untilBeginOfDay == null)
                throw new core.exception.SimAbortedException(
                    new core.report.ErrorMessage(null,
                        "untilBeginOfDay is null! Simulation aborted.",
                        "Class: EntityScheduleEntry  Constructor: untilBeginOfDay is null!",
                        "untilBeginOfDay is null!", "untilBeginOfDay is null!", null));

            if (value == null)
                throw new core.exception.SimAbortedException(
                    new core.report.ErrorMessage(null,
                        "value is null! Simulation aborted.",
                        "Class: EntityScheduleEntry  Constructor: value is null!",
                        "value is null!", "value is null!", null));

            _validFrom = null;
            _validUntil = null;
            _dayOfWeek = EntitySchedule<core.engine.SimProcess>.WorkingDays;
            _timezone = TimeZoneInfo.Utc;
            _fromBeginOfDay = fromBeginOfDay;
            _duration = TimeOperations.diff(untilBeginOfDay, fromBeginOfDay);
            _values = new object[] { value };
        }

        /**
         * Creates a schedule entry with a daily time window and a double payload value.
         */
        public EntityScheduleEntry(Model model, TimeSpan fromBeginOfDay, TimeSpan untilBeginOfDay, double value)
            : this(model, fromBeginOfDay, untilBeginOfDay, (object)value) { }

        /**
         * Creates a schedule entry with a daily time window and default payload value 1.0.
         */
        public EntityScheduleEntry(Model model, TimeSpan fromBeginOfDay, TimeSpan untilBeginOfDay)
            : this(model, fromBeginOfDay, untilBeginOfDay, 1.0) { }

        /**
         * Sets the global validity range of this entry. Outside this range the entry is inactive.
         */
        public void setValidTimeRange(TimeInstant from, TimeInstant until)
        {
            if (TimeInstant.isBeforeOrEqual(from, until))
            {
                _validFrom = from;
                _validUntil = until;
            }
        }

        /**
         * Sets the active days of the week for this entry (index 0=Sun, 1=Mon, …, 6=Sat).
         */
        public void setDayOfWeek(bool[] dayOfWeek)
        {
            _dayOfWeek = new bool[7];
            int m = Math.Max(7, dayOfWeek.Length);
            for (int i = 0; i < m && i < dayOfWeek.Length; i++)
                _dayOfWeek[i] = dayOfWeek[i];
        }

        /** Sets the payload values array. Default is {1.0}. */
        public void setValues(object[] values) => _values = values;

        /** Returns the payload values array. */
        public object[] getValues() => _values;

        /**
         * Returns the start of the calendar day (midnight) for the given simulation time, in the entry's timezone.
         */
        protected TimeInstant getBeginOfDay(TimeInstant time)
        {
            long ms = time.getTimeRounded(TimeUnit.MILLISECONDS);
            DateTimeOffset utc = DateTimeOffset.FromUnixTimeMilliseconds(ms);
            DateTimeOffset local = TimeZoneInfo.ConvertTime(utc, _timezone);
            DateTimeOffset beginOfDay = new DateTimeOffset(
                local.Year, local.Month, local.Day, 0, 0, 0, local.Offset);
            return new TimeInstant(beginOfDay.ToUnixTimeMilliseconds(), TimeUnit.MILLISECONDS);
        }

        /**
         * Returns true if the day-of-week of the given time is active in this entry.
         * Index convention: 0=Sun, 1=Mon, …, 6=Sat (same as DayOfWeek enum cast to int).
         */
        protected bool isValidDayOfWeek(TimeInstant time)
        {
            long ms = time.getTimeRounded(TimeUnit.MILLISECONDS);
            DateTimeOffset utc = DateTimeOffset.FromUnixTimeMilliseconds(ms);
            DateTimeOffset local = TimeZoneInfo.ConvertTime(utc, _timezone);
            return _dayOfWeek[(int)local.DayOfWeek];
        }

        /**
         * Returns true if the given time falls within this entry's active window.
         * The end instant is exclusive.
         */
        internal bool isInEntry(TimeInstant time)
        {
            TimeSpan untilBeginOfDay = TimeOperations.add(_fromBeginOfDay, _duration);
            long days = untilBeginOfDay.getTimeTruncated(TimeUnit.DAYS);
            TimeInstant beginToday = getBeginOfDay(time);
            bool result = false;
            for (int i = 0; i <= days; i++)
            {
                TimeInstant beginDay = TimeOperations.subtract(beginToday, new TimeSpan(i, TimeUnit.DAYS));
                result |= isInEntry(time, beginDay);
            }
            return result;
        }

        private bool isInEntry(TimeInstant time, TimeInstant beginOfDay)
        {
            if (time == null) return false;
            TimeInstant timeFrom = TimeOperations.add(beginOfDay, _fromBeginOfDay);
            TimeInstant timeUntil = TimeOperations.add(timeFrom, _duration);

            if (_validFrom != null && TimeInstant.isBefore(time, _validFrom)) return false;
            if (_validUntil != null && TimeInstant.isAfter(time, _validUntil)) return false;
            if (!isValidDayOfWeek(beginOfDay)) return false;
            if (TimeInstant.isBefore(time, timeFrom)) return false;
            if (TimeInstant.isAfterOrEqual(time, timeUntil)) return false;
            return true;
        }

        /**
         * Returns the next begin time of this entry at or after the given time.
         * Returns null if the entry has already expired or no valid day is found within 8 days.
         */
        internal TimeInstant? getNextBegin(TimeInstant time)
        {
            if (isFinished(time)) return null;

            TimeInstant? later = startLater(time);
            TimeInstant time1 = later ?? time;
            TimeInstant beginOfDay = getBeginOfDay(time1);

            for (int i = 0; i < 8; i++)
            {
                TimeInstant from = TimeOperations.add(beginOfDay, _fromBeginOfDay);
                if (_validUntil != null && TimeInstant.isAfterOrEqual(from, _validUntil)) break;
                if (isValidDayOfWeek(from) && TimeInstant.isAfterOrEqual(from, time))
                    return from;
                beginOfDay = TimeOperations.add(beginOfDay, new TimeSpan(24, TimeUnit.HOURS));
            }
            return null;
        }

        /**
         * Returns the begin of this entry for the day containing the given time,
         * or null if the given time is not within this entry.
         */
        protected TimeInstant? getBegin(TimeInstant time)
        {
            if (!isInEntry(time)) return null;
            TimeInstant beginOfDay = getBeginOfDay(time);
            return TimeOperations.add(beginOfDay, _fromBeginOfDay);
        }

        /**
         * Returns the end of this entry for the day containing the given time,
         * or null if the given time is not within this entry.
         */
        public TimeInstant? getEnd(TimeInstant time)
        {
            TimeInstant? from = getBegin(time);
            return from != null ? TimeOperations.add(from, _duration) : null;
        }

        /** Returns true if the entry's validity range has passed the given time. */
        protected bool isFinished(TimeInstant time)
            => _validUntil != null && TimeInstant.isBefore(_validUntil, time);

        /** Returns the validFrom instant if this entry starts after the given time; otherwise null. */
        protected TimeInstant? startLater(TimeInstant time)
            => (_validFrom != null && TimeInstant.isAfter(_validFrom, time)) ? _validFrom : null;

        /** Returns true if this entry is either finished or has not yet started. */
        protected bool isNotValid(TimeInstant time)
            => isFinished(time) || startLater(time) != null;

        /**
         * Sets the timezone for this entry. Called by EntitySchedule to synchronize
         * all entries to a common timezone.
         */
        protected internal void setTimeZone(TimeZoneInfo timezone) => _timezone = timezone;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("values:(");
            for (int i = 0; i < _values.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                if (_values[i] is double d) sb.AppendFormat("{0:0.0}", d);
                else sb.Append(_values[i]);
            }
            sb.AppendLine(")");

            double fromHours = _fromBeginOfDay.getTimeAsDouble(TimeUnit.HOURS);
            TimeSpan until = TimeOperations.add(_fromBeginOfDay, _duration);
            double untilHours = until.getTimeAsDouble(TimeUnit.HOURS);
            sb.AppendLine($"  time: {fromHours:0.##}h .. {untilHours:0.##}h");

            string[] dayNames = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
            for (int i = 0; i < 7; i++)
                sb.Append(_dayOfWeek[i] ? dayNames[i] : "--").Append(' ');
            sb.AppendLine();

            if (_validFrom != null && _validUntil != null)
                sb.AppendLine($"  valid: {_validFrom} .. {_validUntil}");

            return sb.ToString();
        }
    }
}
