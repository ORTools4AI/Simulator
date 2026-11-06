// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
   * The Splitter takes a given Entity and splits it in the parts defined in the List of SplitterOutputs. The Splitter inherits all behavior from the
   * station except the transport time which is defined in the SplitterOutput.
   * 
   * @param <EIn>
   *            the entity which comes in the splitter
   * @param <EEx>
   *            the entity which leaves the splitter
   */
    public class Splitter<EIn, EEx> : Station<EIn>,
            HasPredecessor<EIn>

        where EIn : Entity
        where EEx : Entity
    {
        /**
         * The List of Splitteroutputs
         */
        protected readonly List<SplitterOutput<EIn, EEx>> outputConfigurations;

        /// <summary>
        /// This Constructor sets the given times, initializes the queues and fills the free service capacity
        /// queue. If a given int is lower than zero the station assumes that there is no resriction. This
        /// applies to incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities
        /// </summary>
        /// <param name="outputConfigurations">
        /// the List of Splitteroutputs which contains the information of the outgoing entities and its
        /// ratio.
        /// </param>
        /// <param name="incomingBufferQueueCapacity">the capatiy of the income queue</param>
        /// <param name="maxEntitiesToHandel">the number of max entities the station can handle</param>
        /// <param name="parallelHandledEntities">
        /// the number of max parallel entities a station can handle
        /// </param>
        /// <param name="setupTime">the time the station needs to set up</param>
        /// <param name="serviceTime">the time the station needs for the service</param>
        /// <param name="recoveryTime">the time the station needs tor recover</param>
        /// <param name="owner">the model owner</param>
        /// <param name="name">the name of the station</param>
        /// <param name="showInReport">should this construct be in the report</param>
        /// <param name="showInTrace">should this construct be in the trace</param>
        public Splitter(List<SplitterOutput<EIn, EEx>> outputConfigurations,
                int incomingBufferQueueCapacity, int maxEntitiesToHandel, int parallelHandledEntities,
                NumericalDist<double> setupTime, NumericalDist<double> serviceTime, NumericalDist<double> recoveryTime,
                Model owner, string name, bool showInReport, bool showInTrace) : base(incomingBufferQueueCapacity, maxEntitiesToHandel, parallelHandledEntities, setupTime,
                    serviceTime, recoveryTime, null, owner, name, showInReport, showInTrace)
        {


            this.outputConfigurations = outputConfigurations;
        }


        protected TimeSpan getTransportTime()
        {
            sendWarning("The Transporttime is defined in the Splitteroutput class", "Splitter : " + getName()
                    + " Method: getTransportTime()", "the transport time is not defnied in the Splitter class.",
                    "dont call this method. Use the Splitteroutput method");
            return null;
        }

        /**
         * schedules the succsessor events which a defined in the Splitteroutput class with the defined transport time
         */
        //@Override
        //@SuppressWarnings({ "unchecked", "rawtypes" })

        protected void onServiceFinisched(EIn who)
        {
            foreach (var outputConfig in outputConfigurations)
            {
                for (int i = 0; i < outputConfig.getNumberOfOutputPartsPerOriginalEntity(); i++)
                {
                    try
                    {
                        Event eventToSchedule;
                        Entity outputEntity;

                        eventToSchedule = outputConfig.getSuccessorEvent().clone();
                        outputEntity = outputConfig.createOutputEntity(who);

                        this.sendTraceNote(getName() + " starts transporting of  "
                                + outputEntity.getName() + " to the successor (next station)");

                        eventToSchedule.schedule(outputEntity, outputConfig.getTransportTime());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            }

        }

    }
}
