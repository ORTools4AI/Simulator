// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.report
{

    /**
     * Represents a message produced by a simulation run whenever entities and / or
     * events are scheduled, created, deleted or enqueued. Tracenotes document the
     * changes of state of the model and allow modellers to trace and check the
     * model's dynamic behaviour. The basic information needed to trace a model's
     * changes of state are:
     * <ul>
     * <li>The name of the Model this message was produced in</li>
     * <li>The point of simulation time that this message was created</li>
     * <li>The name of the event involved in the change of state</li>
     * <li>The name of the entity or process involved in the change of state</li>
     * <li>A text to describe the kind of changes made to the model</li>
     * </ul>
     */
    public class TraceNote : Message
    {

        /**
         * The current entity/entities that produced this message.
         */
        private string _who;

        /**
         * The current Event that produced this message. is especially useful when
         * using multiple models.
         */
        private string _what;

        /// <summary>
        /// Creates a new tracenote with the given parameters as initial values. If <c>null</c> references
        /// are given, they are displayed as "----" in the trace output.
        /// </summary>
        /// <param name="origin">The model that produced this tracenote</param>
        /// <param name="message">The actual trace message</param>
        /// <param name="time">The point in simulation time this tracenote was produced</param>
        /// <param name="entityInvolved">The entity involved in this change of state</param>
        /// <param name="eventInvolved">The event involved in this change of state</param>
        public TraceNote(Model origin, string message, TimeInstant time,
                Entity entityInvolved, EventAbstract eventInvolved) : base(origin, message, time)
        {

            //super(origin, message, time);

            if (entityInvolved == null)
                _who = "----";
            else
                _who = entityInvolved.getName();

            if (eventInvolved == null)
                _what = "----";
            else
                _what = eventInvolved.getName();

        }

        /// <summary>
        /// Creates a new tracenote with the given parameters as initial values. If <c>null</c> references
        /// are given, they are displayed as "----" in the trace output.
        /// </summary>
        /// <param name="origin">The model that produced this tracenote</param>
        /// <param name="message">The actual trace message</param>
        /// <param name="time">The point in simulation time this tracenote was produced</param>
        /// <param name="entitiesInvolved">The entities involved in this change of state</param>
        /// <param name="eventInvolved">The event involved in this change of state</param>
        public TraceNote(Model origin, string message, TimeInstant time,
                List<Entity> entitiesInvolved, EventAbstract eventInvolved) : base(origin, message, time)
        {

            //super(origin, message, time);

            if (entitiesInvolved == null || entitiesInvolved.Count == 0)
                _who = "----";
            else
            {
                _who = entitiesInvolved[0].getQuotedName();
                if (entitiesInvolved.Count > 1)
                {
                    _who += entitiesInvolved.Count > 2 ?
                            ", " + entitiesInvolved[1].getQuotedName() : " and " + entitiesInvolved[1].getQuotedName();
                    if (entitiesInvolved.Count > 2)
                    {
                        _who += " and " + entitiesInvolved[2].getQuotedName();
                    }
                }
            }

            if (eventInvolved == null)
                _what = "----";
            else
                _what = eventInvolved.getQuotedName();

        }

        /// <summary>Returns the name of the entity this tracenote evolved from.</summary>
        /// <returns>The name of the entity that produced this tracenote</returns>
        public string getEntity()
        {

            return _who;

        }

        /// <summary>Returns the name of the event described in this tracenote from.</summary>
        /// <returns>The name of the event that produced this tracenote</returns>
        public string getEvent()
        {

            return _what;

        }
    }
}