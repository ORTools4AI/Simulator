// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;


namespace ORTools4AI.Simulator.extensions.chaining.abstractions
{
    /**
  * Interface which indicates that a station has a successor
  * 
  * @param <E>
  *            The Entitiy which can be handled by the station
  */
    public interface HasSuccessor<E> where E:Entity
    {

        /// <summary>This method is used to set the successor event</summary>
        /// <param name="eventToScheduleWhenFinished">the event to be set as successor event</param>
        void setSuccessor(Event eventToScheduleWhenFinished);

        /// <summary>This method is used to set the successor event</summary>
        /// <param name="successor">the successor of the current station</param>
        void setSuccessor(HasPredecessor<E> successor);

    }
}
