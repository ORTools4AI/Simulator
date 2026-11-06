// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
 * Helper class to setup successor behaviour. 
 */
    public class SuccessorAdministration<E> where E:Entity
     {

        /**
         * the successor event to be scheduled
         */
        private Event successorEvent;
        private readonly Model owner;
        private readonly string location;

        public SuccessorAdministration(Model owner, string location)
        {
            this.owner = owner;
            this.location = location;
        }

        public void setSuccessor(Event eventToScheduleWhenFinished)
        {
            successorEvent = eventToScheduleWhenFinished;
        }

        public void setSuccessor(HasPredecessor<E> successor)
        {
            setSuccessor(successor.getStartEvent());
        }

        public Event getSuccessorEvent()
        {
            if (successorEvent != null)
            {
                return successorEvent;
            }
            else
            {
                owner.sendWarning(
                        "Not successor scheduled.",
                        location,
                        "No successor has been scheduled for this chainable construct. A dummy event is created to avaiod a NullPointerException.",
                        "Please set a successor for all your chainable constructs.");
                return new SendTraceNoteEvent(owner, "DummyEvent", true)
                {

                };
            }
        }
        private class SendTraceNoteEvent : Event
        {
            public SendTraceNoteEvent(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
            {
            }

            public override void eventRoutine(Entity who)
            {
                sendTraceNote("Dummy event executed because no successor(event) was set.");
            }
        }

    }

}
