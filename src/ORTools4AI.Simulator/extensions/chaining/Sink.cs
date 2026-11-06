// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using ORTools4AI.Simulator.extensions.chaining.report;


namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
 * The Sink represents the end of a production line and has no successor. The
 * Sink inherits all behavior from the Station except the transport time.
 * 
 * @param <E>
 *            The Entitiy which can be handled by the Sink
 */
    public class Sink<E>: Station<E> ,
            HasPredecessor<E>  where E:Entity
    {
        /// <summary>
        /// This Constructor sets the given times, initializes the queues and fills the free service capacity
        /// queue. If a given int is lower than zero the sink assumes that there is no restriction. This
        /// applies to incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities
        /// </summary>
        /// <param name="incomingBufferQueueCapacity">the capatiy of the income queue</param>
        /// <param name="maxEntitiesToHandle">the number of max entities the station can handle</param>
        /// <param name="parallelHandledEntities">
        /// the number of max parallel entities a station can handle
        /// </param>
        /// <param name="setupTime">the time the station needs to set up</param>
        /// <param name="serviceTime">the time the station needs for the service</param>
        /// <param name="recoveryTime">the time the station needs to recover</param>
        /// <param name="owner">the model owner</param>
        /// <param name="name">the name of the station</param>
        /// <param name="showInReport">should this construct be in the report</param>
        /// <param name="showInTrace">should this construct be in the trace</param>
        public Sink(int incomingBufferQueueCapacity, int maxEntitiesToHandle,
        int parallelHandledEntities, NumericalDist<double> setupTime,
                NumericalDist<double> serviceTime, NumericalDist<double> recoveryTime,
                Model owner, string name, bool showInReport, bool showInTrace):base(incomingBufferQueueCapacity, maxEntitiesToHandle,
                    parallelHandledEntities, setupTime, serviceTime, recoveryTime,
                    null, owner, name, showInReport, showInTrace)
        {

            
        }

     
    
        protected SmartReporter createDefaultReporter()
        {

            SmartReporter reporter = base.createDefaultReporter();
            reporter.overrideValueAt("N/A", 2);
            return reporter;
        }

    }
}
