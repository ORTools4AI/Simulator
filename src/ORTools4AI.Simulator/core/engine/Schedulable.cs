// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using System.Xml.Linq;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{


    /**
     * All objects that want to be handled by the scheduler must extend this class.
     * These are events (including external events), entities and SimProcesses. All
     * functionality common to Schedulable objects are encapsulated here.
     */
    public class Schedulable : ModelComponent
    {

        /**
         * A list containing all scheduled entries for this Schedulable in future.
         */
        protected List<EventNote> _schedule;

        /// <summary>
        /// Constructs a Schedulable object with the given name. Note that to identify the multiple objects
        /// that can be created of one type of Schedulable, each has a unique number added to its name as a
        /// suffix. The numbers represent the amount of objects being created with the same name.
        /// </summary>
        /// <param name="owner">The owner of this Schedulable object</param>
        /// <param name="name">The name of the new Schedulable object</param>
        public Schedulable(Model owner, string name, bool showInTrace) : base(owner, owner.getExperiment().getNameCatalog()
                    .registeredName(name), showInTrace)
        {

            // create modelcomponent with the checked and registered name

            this._schedule =  new List<EventNote>();
        }


        /**
         * Removes all events scheduled for this entity from the event-list. 
         * The Entity will not become active until scheduled again.
         */
        public void cancel()
        {
            if (!isScheduled())
            {
                sendWarning("Can't cancel Schedulable! Command ignored.",
                        "Schedulable : " + getName() + " Method: void cancel()",
                        "The Schedulable to be canceled is not scheduled.",
                        "Use method isSchedule() to test if the Schedulable "
                                + "is scheduled and thus can be canceled or not.");
                return; // was already scheduled
            }

            // removes all scheduled events for this entity by using global event-list
            while (_schedule.Count > 0)
            {
                //EventNote firstNote = this.getModel().getExperiment().getScheduler().evList.firstNote();
                EventNote firstNote = _schedule.ElementAt(0);

                if (currentlySendTraceNotes())
                {
                    TimeInstant time = firstNote.getTime();
                    if (this == current())
                    {
                        if (time == presentTime())
                            sendTraceNote("cancels scheduled Event " + firstNote.getEvent() + " for itself, which was scheduled now");
                        else
                            sendTraceNote("cancels scheduled Event " + firstNote.getEvent() + " for itself, which was scheduled at " + time);
                    }
                    else
                    {
                        if (time == presentTime())
                            sendTraceNote("cancels scheduled Event " + firstNote.getEvent() + " for " + this.getName() + ", which was scheduled now");
                        else
                            sendTraceNote("cancels scheduled Event " + firstNote.getEvent() + " for " + this.getName() + ", which was scheduled at " + time);
                    }
                }
                this.getModel().getExperiment().getScheduler().evList.remove(firstNote);
            }
        }


        /// <summary>
        /// Returns a list of EventNote associated to this Entity object. If the Entity object is not
        /// currently scheduled, <c>null</c> will be returned. Remind that all different Event classes can be
        /// included.
        /// </summary>
        /// <returns>
        /// The event-notes associated to the entity or <c>null</c> if Entity is not currently scheduled
        /// </returns>
        internal List<EventNote> getEventNotes()
        {
            return _schedule;

        }

        /// <summary>Shows if this Schedulable is the currently active object.</summary>
        /// <returns><c>true</c> if this Schedulable is the currently active, <c>false</c> otherwise</returns>
        public bool isCurrent()
        {

            return this.Equals(getModel().getExperiment().getScheduler()
                    .getCurrentSchedulable());

        }

        /// <summary>Tests if this entity has already been scheduled.</summary>
        /// <returns><c>true</c> if already scheduled, <c>false</c> otherwise</returns>
        public bool isScheduled()
        {

            // Not associated to EventNote if not scheduled
            return _schedule.Count > 0;

        }

        /// <summary>Removes an event note from this object's schedule.</summary>
        /// <param name="note">The event note to remove.</param>
        internal void removeEventNote(EventNote note)
        {
            _schedule.Remove(note); // only removes Event in local list

        }

        /// <summary>
        /// Allows to rename a Schedulable object while keeping its internally added serial number to keep
        /// track of the individual object. Note that invalid string parameters will result in renaming the
        /// Schedulable to 'unnamed'.
        /// </summary>
        /// <param name="newName">The Schedulable's new name</param>
        public void rename(string newName)
        {

            base.rename(getModel().getExperiment().getNameCatalog()
                    .registeredName(newName));

        }

        /// <summary>
        /// Re-schedules the Schedulable by shifting all EventNote by a specified <c> TimeSpan</c>.
        /// </summary>
        /// <param name="dt">
        /// The offset to the current simulation time at which this Schedulable is to be re-scheduled
        /// </param>
        public void reSchedule(TimeSpan dt)
        {
            if (dt == null)
            {
                sendWarning("Can't reSchedule Schedulable! Command ingnored.",
                        "Schedulable : " + getName()
                                + " Method: reSchedule(TimeSpan dt)",
                        "The simulation time given as parameter is a null "
                                + "reference.",
                        "Be sure to have a valid TimeSpan reference before calling "
                                + "this method.");
                return; // no proper parameter
            }

            if (!isScheduled())
            {
                sendWarning("Can't reSchedule Schedulable! Command ingnored.",
                        "Schedulable : " + getName()
                                + " Method: reSchedule(TimeSpan dt)",
                        "The Schedulable is not scheduled, thus unable to be "
                                + "reScheduled..",
                        "Be sure that the Schedulable is currently scheduled "
                                + "before calling this method.");
                return; // no proper parameter
            }

            // create list with new events and remove old events
            EventNote newNote = null;
            var oldNotes = new List<EventNote>(_schedule);
            var newNotes = new List<EventNote>();
            foreach (EventNote oldNote in oldNotes)
            {
                newNote = oldNote.copy(current()); // copy EventNote
                newNote.setTime(TimeOperations.add(oldNote.getTime(), dt)); // shift it
                newNotes.Add(newNote); // save it to new list
                this.getModel().getExperiment().getScheduler().evList.remove(oldNote); // remove original event-note local and global

                if (currentlySendTraceNotes())
                {
                    TimeInstant timeOld = oldNote.getTime();
                    TimeInstant timeNew = newNote.getTime();
                    if (this == current())
                    {
                        if (timeOld == presentTime())
                            sendTraceNote("reschedules " + newNote.getEvent() + " for itself, which was scheduled now, to " + timeNew);
                        else
                            sendTraceNote("reschedules " + newNote.getEvent() + " for itself, which was scheduled at " + timeOld + ", to " + timeNew);
                    }
                    else
                    {
                        if (timeOld == presentTime())
                            sendTraceNote("reschedules " + newNote.getEvent() + " for " + this.getName() + ", which was scheduled now, to " + timeNew);
                        else
                            sendTraceNote("reschedules " + newNote.getEvent() + " for " + this.getName() + ", which was scheduled at " + timeOld + ", to " + timeNew);
                    }
                }

            }

            //insert temp to schedule
            foreach (EventNote ev in newNotes)
            {
                // insert to GLOBAL list, which inserts to local one
                this.getModel().getExperiment().getScheduler().evList.insert(ev);
            }

        }

        /// <summary>
        /// Re-schedules the Schedulable to some other point in simulation time (which should be different
        /// form the instant at which is a scheduled at the moment).
        /// </summary>
        /// <param name="time">The simulation time at which this entity is to be re-scheduled</param>
        public void reSchedule(TimeInstant time)
        {

            if (time == null)
            {
                sendWarning("Can't reSchedule enitty! Command ingnored.",
                        "Entity : " + getName()
                                + " Method: reSchedule(TimeInstant time)",
                        "The simulation time given as parameter is a null "
                                + "reference.",
                        "Be sure to have a valid TimeSpan reference before calling "
                                + "this method.");
                return; // no proper parameter
            }

            if (!isScheduled())
            {
                sendWarning("Can't reSchedule Schedulable! Command ingnored.",
                        "Entity : " + getName()
                                + " Method: reSchedule(TimeInstant time)",
                        "The Entity is not scheduled, thus unable to be "
                                + "reScheduled.",
                        "Be sure that the entity is currently scheduled "
                                + "before calling this method.");
                return; // no proper parameter
            }

            if (_schedule.Count() > 1)
            {
                sendWarning("Can't reSchedule Entity! Command ingnored.",
                        "Entity : " + getName()
                                + " Method: reSchedule(TimeInstant time)",
                        "The Entity is scheduled more then once, thus unable to be "
                                + "reScheduled.",
                        "Be sure that the entity is only scheduled once"
                                + "before calling this method.");
                return; // no proper parameter
            }

            if (TimeInstant.isBefore(time, this.presentTime()))
            {
                sendWarning(
                        "Can't reschedule Entity at given time! "
                                + "Command ignored.",
                        "Entity : "
                                + getName()
                                + " Method: reSchedule(TimeInstant time)",
                        "The instant given is in the past.",
                        "To reschedule an entity, use a TimeInstant no earlier than the present time. "
                                + "The present time can be obtained using the "
                                + "presentTime() method");
                return;
                // I can't be rescheduled, TimeInstant has already passed.
            }

            if (currentlySendTraceNotes())
            {
                if (this == current())
                {
                    if (time == presentTime())
                        sendTraceNote("reschedules itself now");
                    else
                        sendTraceNote("reschedules itself at "
                                + time);
                }
                else
                {
                    if (time == presentTime())
                        sendTraceNote("reschedules '" + getName() + "' now");
                    else
                        sendTraceNote("reschedules '" + getName() + "' at "
                                + time);
                }
            }

            getModel().getExperiment().getScheduler().reScheduleNoPreempt(this, time);

            if (currentlySendDebugNotes())
            {
                sendDebugNote("reschedules on EventList<br>"
                        + getModel().getExperiment().getScheduler().toString());
            }

        }

        /// <param name="dt">
        /// The offset to the current simulation time at which this Schedulable is to be re-scheduled
        /// </param>
        /// <remarks>
        /// Use reSchedule(TimeSpan dt). Re-schedules the Schedulable at some other point in simulation time
        /// than it already had been scheduled before.
        /// </remarks>
        public void reSchedule(SimTime dt)
        {

            reSchedule(SimTime.toTimeSpan(dt));

        }

        /// <returns>
        /// The point of simulation time this Schedulable is scheduled at or <c>null</c> otherwise
        /// </returns>
        /// <remarks>
        /// Returns the point of simulation time this Schedulable is scheduled at. Returns null, if the
        /// entity has not been scheduled and thus can not return an appropriate point of simulation time.
        /// Use scheduleNext() for next TimeInstant where an event-note lists this entity.
        /// </remarks>
        public TimeInstant scheduledAt()
        {

            return scheduledNext(); //use scheduleNext()

        }

        /// <summary>Returns the next point of time this entity is scheduled.</summary>
        /// <returns>
        /// The point of simulation time the next Entity is schedule or <c>null</c> otherwise
        /// </returns>
        public TimeInstant scheduledNext()
        {
            if (!isScheduled())
            {
                return null; // if not scheduled, there is no point of time
            }
            else
            {
                return _schedule.ElementAt(0).getTime();
            }
        }


        /**
         * Adds an event-note to the Schedulable.
         */
        internal void addEventNote(EventNote note)
        {
            if (!_schedule.Contains(note))
            {
                _schedule.Add(note);
                _schedule.Sort();
            }
        }

        /// <summary>Creates a shallow copy with a new name and an empty schedule.</summary>
        protected Schedulable clone()
        {
            Schedulable c = (Schedulable)base.MemberwiseClone();
            NameCatalog nc = this.getModel().getExperiment().getNameCatalog();
            c.rename(nc.getNameWithoutSuffix(this.getName()));
            c._schedule = new List<EventNote>();
            return c;
        }

        public void reSchedulePreempt()
        {
            if (getModel().getExperiment().getScheduler().getCurrentSimProcess() == null) {
                sendWarning("Can't preempt current SimProcess! Command ignored.",
                    "Schedulable : " + getName() + " Method: reSchedulePreempt()",
                    "No current SimProcess to preempt.",
                    "Call this method during SimProcess execution only.");
                return;
            }
            if (!isScheduled()) {
                sendWarning("Can't reSchedule Schedulable! Command ignored.",
                    "Schedulable : " + getName() + " Method: reSchedulePreempt()",
                    "The Schedulable is not scheduled, thus unable to be reScheduled.",
                    "Be sure that the Schedulable is currently scheduled before calling this method.");
                return;
            }
            if (_schedule.Count > 1) {
                sendWarning("Can't reSchedule Schedulable! Command ignored.",
                    "Schedulable : " + getName() + " Method: reSchedulePreempt()",
                    "The Schedulable is scheduled more than once, thus unable to be reScheduled.",
                    "Be sure that the Schedulable is only scheduled once before calling this method.");
                return;
            }
            if (currentlySendTraceNotes()) {
                if (this == current()) sendTraceNote("reschedules itself now");
                else sendTraceNote("reschedules '" + getName() + "' now");
            }
            getModel().getExperiment().getScheduler().reScheduleWithPreempt(this);
            if (currentlySendDebugNotes())
                sendDebugNote("reschedules on EventList<br>"
                    + getModel().getExperiment().getScheduler().toString());
        }

        private Reportable _correspondingReportable = null;

        public void assignReporter(Type reporterClass, bool showInReport, bool showInTrace)
        {
            var d = new DummyReportable(this.getModel(), reporterClass, showInReport, showInTrace, this);
            this.setCorrespondingReportable(d);
            d.setCorrespondingSchedulable(this);
        }

        public Reportable getCorrespondingReportable() => _correspondingReportable;

        public void setCorrespondingReportable(Reportable correspondingReportable)
        {
            if (correspondingReportable != null && this.getModel() != correspondingReportable.getModel()) {
                this.sendWarning(
                    "Reportable to correspond to this Schedulable must belong to the same model!",
                    "Schedulable.setCorrespondingReportable(Reportable)",
                    "Model of Schedulable and corresponding Reportable must be identical.",
                    "Do not set a corresponding Reportable from another model.");
                return;
            }
            this._correspondingReportable = correspondingReportable;
        }

        private class DummyReportable : Reportable
        {
            private readonly Type _reporterClass;
            private readonly Schedulable _owner;

            public DummyReportable(Model owner, Type reporterClass, bool showInReport, bool showInTrace, Schedulable schedulableOwner)
                : base(owner, "DummyReporter", showInReport, showInTrace)
            {
                _reporterClass = reporterClass;
                _owner = schedulableOwner;
            }

            protected override Reporter createDefaultReporter()
            {
                try {
                    return (Reporter)Activator.CreateInstance(_reporterClass, _owner);
                } catch (Exception) {
                    this.sendWarning(
                        "Instanciating the user-specified reporter caused an exception. Using default instead.",
                        "Schedulable : " + getName() + " Method: DummyReportable.createDefaultReporter()",
                        "User-specified class is not accessible or constructor cannot be invoked.",
                        "Make sure the reporter class has a constructor taking a Schedulable as its only parameter.");
                    return base.createDefaultReporter();
                }
            }
        }
    }
}