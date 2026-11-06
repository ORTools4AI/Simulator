// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using System.Text;

namespace ORTools4AI.Simulator.core.engine
{

    /**
     * Provides the abstract super class for user defined events to change an entity's
     * internal state. The state of a discrete model is changed by events that occur
     * at distinct points of simulation time.
     * <p>
     * an event with up to three entities, changing its state according to the
     * entity's reaction to the specific Event in the system under inspection. So
     * each type of Event acting on one certain type of Entity requires a new
     * subclass to be derived from this class. Since events are associated to a
     * single entity, the method executing the changes of state of a specific Entity
     * gets that entity passed as a parameter. The scheduler takes care that this is
     * done at the specified point of simulation time.
     * <p>
     * For type safety it is recommended to generically assign the entity type an
     * Event operates on by using the generic type
     * <code>Event&lt;EntityOperatingOn&gt;</code> where
     * <code>EntityOperatingOn</code> is derived from <code>Entity</code>.
     * <p>
     * events should be used one time only. They are created to be scheduled
     * together with a specific Entity, change that entity's state at the scheduled
     * point of simulation time and are destroyed by Java's garbage collector after
     * use. They could be reused but at a certain risk of inconsistent states. Since
     * each object of a class that is derived from the class
     * <code>Schedulable</code> has its unique identification number added as a
     * suffix to its name, reusing Event objects would make one event responsible
     * for several distinct changes in a model at different simulation times. This
     * comes with the danger of of confusing the model's trace and making it more
     * difficult to debug a faulty model implementation. Each type of Event needed
     * for a model requires a new subclass of Event to be derived by the user.
     * <p>
     * Embed the changes of state for the specific Entity associated with this event
     * by overriding the abstract method <code>eventRoutine(Entity e)</code>. Events
     * that do not manipulate a single entity but act on the model's state on a more
     * general matter are defined by external events, a subclass of this class.
     * 
     * @see Entity
     * @see ExternalEvent
     * @see TimeInstant
     * @see TimeSpan
     */
    public abstract class EventAbstract : Schedulable
    {
        /**
         * The realTime deadline for this event in nanoseconds. In case of a
         * real-time execution (i. e. the execution speed rate is set to a positive
         * value) the Scheduler will produce a warning message if a deadline is
         * missed.
         */
        private long _realTimeConstraint;

        /**
         * The number of entities is saved here.
         */
        protected long numberOfEntities;

        /**
         * The scheduling priority of the event.
         */
        private int _mySchedulingPriority;

        /// <summary>Creates a new event of the given model, with the given name and trace option.</summary>
        /// <param name="owner">The model this event is associated to</param>
        /// <param name="name">The name of this event</param>
        /// <param name="showInTrace">
        /// Flag for showing event in trace-files. Set it to <c>true</c> if event should show up in trace.
        /// Set it to <c>false</c> if event should not be shown in trace.
        /// </param>
        public EventAbstract(Model owner, String name, bool showInTrace) : base(owner, name, showInTrace)
        {

            //super(owner, name, showInTrace);
            numberOfEntities = 0;
            _mySchedulingPriority = 0;

        }

        /// <summary>
        /// Tests if this event actually is an external event which is not used for modelling but to control
        /// the experiment to act at certain points of simulation time. External events must not be connected
        /// to an entity.
        /// </summary>
        /// <returns>
        /// Is <c>true</c> if this is an instance of class <c>ExternalEvent</c>,<c>false</c> otherwise
        /// </returns>
        public bool isExternal()
        {

            //return (this instanceof ExternalEvent);
            return false;
        }


        /// <summary>
        /// Returns the realTime deadline for this event (in nanoseconds). In case of a real-time execution
        /// (i. e. the execution speed rate is set to a positive value) the Scheduler will produce a warning
        /// message if a deadline is missed.
        /// </summary>
        /// <returns>the realTimeConstraint in nanoseconds</returns>
        public long getRealTimeConstraint()
        {
            return _realTimeConstraint;
        }

        /// <summary>
        /// Returns the realTime deadline for this event (in nanoseconds). In case of a real-time execution
        /// (i. e. the execution speed rate is set to a positive value) the Scheduler will produce a warning
        /// message if a deadline is missed.
        /// </summary>
        /// <returns>the realTimeConstraint in nanoseconds</returns>
        public long getNumberOfEntities()
        {
            return numberOfEntities;
        }

        /// <summary>
        /// Sets the realTime deadline for this event (in nanoseconds). In case of a real-time execution (i.
        /// e. the execution speed rate is set to a positive value) the Scheduler will produce a warning
        /// message if a deadline is missed.
        /// </summary>
        /// <param name="realTimeConstraint">the realTimeConstraint in nanoseconds to set</param>
        public void setRealTimeConstraint(long realTimeConstraint)
        {
            this._realTimeConstraint = realTimeConstraint;
        }

        /// <summary>
        /// Utility method to generate trace output for scheduling this event (internal use only).
        /// </summary>
        /// <param name="who1">
        /// the first entity scheduled with this event (or <c>null</c> if not applicable)
        /// </param>
        /// <param name="who2">
        /// the second entity scheduled with this event (or <c>null</c> if not applicable)
        /// </param>
        /// <param name="who3">
        /// the third entity scheduled with this event (or <c>null</c> if not applicable)
        /// </param>
        /// <param name="after">
        /// the Schedulable after which this event is scheduled (or <c>null</c> if not applicable)
        /// </param>
        /// <param name="before">
        /// the Schedulable before which this event is scheduled (or <c>null</c> if not applicable)
        /// </param>
        /// <param name="at">the TimeInstant at which this event is scheduled</param>
        /// <param name="remark">optional further comments (or <c>null</c> if not applicable)</param>
        protected void generateTraceForScheduling(Entity who1, Entity who2, Entity who3, Schedulable after, Schedulable before, TimeInstant at, String remark)
        {

            if (currentlySendTraceNotes())
            {

                StringBuilder trace = new StringBuilder("schedules '" + getName() + "'");
                if (who1 != null)
                {
                    String who1alias = (who1 == currentEntity() && who2 == null && who3 == null ? "itself" : "'" + who1.getName() + "'");
                    trace.Append(" of " + who1alias);
                    if (who2 != null)
                    {
                        trace.Append(who3 == null ? " and '" + who2.getName() + "'" : ", '" + who2.getName() + "'");
                        if (who3 != null)
                        {
                            trace.Append(" and '" + who3.getName() + "'");
                        }
                    }
                }

                if (after != null)
                {
                    String afterAlias = (after == currentEntity() ? "itself" : "'" + after.getName() + "'");
                    trace.Append(" after " + afterAlias);
                }
                else if (before != null)
                {
                    String beforeAlias = (before == currentEntity() ? "itself" : "'" + before.getName() + "'");
                    trace.Append(" before " + beforeAlias);
                }

                if (at == this.presentTime())
                {
                    trace.Append(" now");
                }
                else
                {
                    trace.Append(" at " + at.toString());
                }

                if (remark != null && remark.Length > 0)
                {
                    trace.Append(". " + remark);
                }

                this.sendTraceNote(trace.ToString());
            }
        }

        /// <summary>
        /// Returns the event's scheduling priority. The scheduling priority is used to determine which event
        /// to execute first if two or more events are scheduled at the same instant. The default priority is
        /// zero. Higher priorities are positive, lower priorities negative.
        /// </summary>
        /// <returns>The events's priority</returns>
        public int getSchedulingPriority()
        {

            return _mySchedulingPriority;

        }

        /// <summary>
        /// Sets the entity's scheduling priority to a given integer value. The default priority (unless
        /// assigned otherwise) is zero. Negative priorities are lower, positive priorities are higher. All
        /// values should be inside the range defined by the C# <c>int</c> type
        /// [-2147483648, +2147483647]. An event's scheduling priority it used to determine which event is
        /// executed first if scheduled for the same time instant. Should the priority be the same, event
        /// execution depends on the <c>EventList</c> in use, e.g. scheduled first is executed first
        /// (<c>EventTreeList</c>) or random (<c>RandomizingEventTreeList</c>).
        /// </summary>
        /// <param name="newPriority">The new scheduling priority value</param>
        public void setSchedulingPriority(int newPriority)
        {

            this._mySchedulingPriority = newPriority;

        }

        /// <summary>Creates a copy of the event with an empty schedule.</summary>
        protected EventAbstract clone()
        {
            return (EventAbstract)base.clone();
        }
    }
}