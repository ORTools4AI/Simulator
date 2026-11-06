// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
    * This class is used by the Merger construct for composing the ratio of entities needed for the merging process. The Mergerconfig is a Hashmap with
    * key=The class of the Entity and Value= an integer of the number of required entities.
    *
    * @param <E>
    *            The Entitiy which can be handled by the station
    */
    // @SuppressWarnings("serial")
    public class MergerConfig<E> : Dictionary<Type, int> where E : Entity
    {

        public MergerConfig()
        {

        }

        /// <summary>
        /// The Mergerconfig can be initialized with a Map instead of putting the Key/Value pairs in the map.
        /// </summary>
        /// <param name="mergerParameters">
        /// Dictionary with key=runtime Type of the Entity and Value=required count.
        /// </param>
        public MergerConfig(Dictionary<Type, int> mergerParameters) : base(mergerParameters)
        {

        }

        /**
         * Convenience: add a required entity type by generic parameter.
         */
        public void require<T>(int count) where T : E
        {
            this[typeof(T)] = count;
        }

    }
}
