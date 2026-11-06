// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.5.1e copyright (c) 2017
// @author christian.mueller@th-wildau.de
// Modified for ORTools4AI.Simulator.

using System.Collections;
using ORTools4AI.Simulator.core.engine;
using System;
using System.Collections.Generic;
using System.Text;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.scheduling
{
    /**
     * EntitySchedule is a list of EntityScheduleEntry objects.
     * An EntityScheduleEntry defines a recurring time window (e.g. every working day 08:00–16:00).
     * All entries share the same timezone stored in EntitySchedule.
     *
     * EntitySchedule is a SimProcess. A list of SimProcess objects can be registered via
     * addProcess(). The lifeCycle activates each registered process at the start of every
     * window and (optionally) cancels them at the end.
     */
    public class EntitySchedule<E> : SimProcess where E : SimProcess
    {
        // Preset day-of-week arrays (index 0=Sun, 1=Mon, …, 6=Sat)
        public static readonly bool[] WorkingDays = { false, true, true, true, true, true, false };
        public static readonly bool[] Weekend      = { true,  false, false, false, false, false, true };
        public static readonly bool[] AllDays      = { true,  true,  true,  true,  true,  true,  true };

        private readonly List<EntityScheduleEntry> _entryList   = new();
        private TimeZoneInfo                        _timezone    = TimeZoneInfo.Utc;
        private readonly Model                      _model;
        private readonly List<E>                    _processList = new();
        private bool                                _processActivate   = true;
        private bool                                _processPassivate  = false;

        /**
         * Creates an EntitySchedule with one registered process.
         * By default: activates processes at window start, does not cancel them at window end.
         */
        public EntitySchedule(Model owner, string name, E? process) : base(owner, name, owner.getExperiment().traceIsOn())
        {
            _model = owner;
            if (process != null) _processList.Add(process);
        }

        /** Creates an EntitySchedule with no registered process. */
        public EntitySchedule(Model owner) : this(owner, "", null) { }

        /**
         * Creates an EntitySchedule with a single entry covering the given daily window on working days.
         */
        public EntitySchedule(Model owner, TimeSpan fromBeginOfDay, TimeSpan untilBeginOfDay) : this(owner)
        {
            add(new EntityScheduleEntry(owner, fromBeginOfDay, untilBeginOfDay));
        }

        /**
         * Configures whether registered processes are activated at window start
         * and/or cancelled at window end.
         */
        public void setProcessManagement(bool activate, bool passivate)
        {
            _processActivate  = activate;
            _processPassivate = passivate;
        }

        /** Sets the timezone shared by all entries in this schedule. */
        public void setTimeZone(TimeZoneInfo timezone)
        {
            _timezone = timezone;
            foreach (var entry in _entryList)
                entry.setTimeZone(timezone);
        }

        /** Removes all registered processes. */
        public void clearProcessList() => _processList.Clear();

        /** Registers a process to be managed by this schedule. */
        public void addProcess(E process) => _processList.Add(process);

        /** Unregisters a process from this schedule. */
        public void removeProcess(E process) => _processList.Remove(process);

        /**
         * Adds a schedule entry. The entry's timezone is synchronised to this schedule's timezone.
         */
        public void add(EntityScheduleEntry entry)
        {
            entry.setTimeZone(_timezone);
            _entryList.Add(entry);
        }

        /**
         * Returns the values array of the first entry that contains the given time,
         * or null if no entry matches.
         */
        public object[]? isInSchedule(TimeInstant time)
        {
            foreach (var entry in _entryList)
                if (entry.isInEntry(time)) return entry.getValues();
            return null;
        }

        /**
         * Returns the values array of the first entry that contains the current simulation time,
         * or null if no entry matches.
         */
        public object[]? isInSchedule() => isInSchedule(_model.presentTime());

        /**
         * Returns the end of the active window that contains the given time,
         * or null if not currently in any window.
         */
        public TimeInstant? getEndOfActualEntry(TimeInstant now)
        {
            foreach (var entry in _entryList)
            {
                TimeInstant? end = entry.getEnd(now);
                if (end != null) return end;
            }
            return null;
        }

        /**
         * Returns the earliest next window begin across all entries, strictly after the given time.
         * Returns null if no future window exists.
         */
        public TimeInstant? getBeginOfNextEntry(TimeInstant now)
        {
            TimeUnit epsUnit = _model.getExperiment().getEpsilonUnit();
            TimeInstant big  = new TimeInstant(long.MaxValue - 1, epsUnit);
            TimeInstant min  = big;

            foreach (var entry in _entryList)
            {
                TimeInstant? nextBegin = entry.getNextBegin(now);
                if (nextBegin != null && TimeInstant.isBefore(nextBegin, min))
                    min = nextBegin;
            }

            return min.Equals(big) ? null : min;
        }

        /**
         * Returns true if the entity's numeric suffix is within the capacity allowed by
         * values[index] of the currently active schedule entry.
         * Expects entity names to end with "#N".
         */
        public bool checkAvailability(Entity e, int index)
        {
            long available = 0;
            object[]? vals = isInSchedule();
            if (vals != null)
            {
                object tmp = vals[index];
                if (tmp is double d)       available = (long)Math.Round(d);
                else if (tmp is long l)    available = l;
                else
                    throw new core.exception.SimAbortedException(
                        new core.report.ErrorMessage(null,
                            "Can't checkAvailability! Simulation aborted.",
                            "Class: EntitySchedule  Method: checkAvailability  value[" + index + "] is not Double or Long",
                            "Its type is: " + tmp.GetType().Name,
                            "Ensure values[" + index + "] stores a Double or Long.", null));
            }
            string name = e.GetType().Name + ":" + e.getName();
            int entityIndex = int.Parse(name.Substring(name.IndexOf('#') + 1).Trim());
            return entityIndex <= available;
        }

        /** Overload with default index 0. */
        public bool checkAvailability(Entity e) => checkAvailability(e, 0);

        /**
         * Main lifecycle: waits for schedule windows and activates/deactivates managed processes.
         */
        protected override IEnumerable Run()
        {
            // If we start mid-window, handle the current window's remainder first.
            TimeInstant? entryEnd = getEndOfActualEntry(_model.presentTime());
            if (entryEnd != null)
            {
                doActivate();
                hold(TimeOperations.diff(entryEnd, _model.presentTime()));
                doPassivate();
            }

            while (_entryList.Count > 0)
            {
                TimeInstant? entryBegin = getBeginOfNextEntry(_model.presentTime());
                if (entryBegin == null) break;

                hold(TimeOperations.diff(entryBegin, _model.presentTime()));
                doActivate();

                entryEnd = getEndOfActualEntry(_model.presentTime());
                if (entryEnd == null) break;
                hold(TimeOperations.diff(entryEnd, _model.presentTime()));

                doPassivate();
            }
            yield break;
        }

        private void doActivate()
        {
            if (!_processActivate) return;
            foreach (E process in _processList)
            {
                if (!process.isScheduled())
                    process.activate();
            }
        }

        private void doPassivate()
        {
            if (!_processPassivate) return;
            // Cancelling a scheduled process removes it from the event queue —
            // the closest available substitute for Java's passivate() in this port.
            foreach (E process in _processList)
            {
                if (process.isScheduled())
                    process.cancel();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in _entryList)
                sb.Append(entry.ToString()).Append("<br>");
            sb.Append(_timezone.DisplayName);
            return sb.ToString();
        }
    }
}
