// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;


namespace ORTools4AI.Simulator.extensions.chaining.abstractions
{
    /**
   * Interface which indicates that an station has a predecessor
   * 
   * @param <E>
   *            The Entity which can be handled by the station
   */
    public interface HasPredecessor<E> where E: Entity
    {

        /// <summary>
        /// This Method returns the start event of a Station which is used to set the successor
        /// </summary>
        /// <returns>the star tevent</returns>
        Event getStartEvent();

    }

}
