// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeSpan = ORTools4AI.Simulator.core.engine.TimeSpan;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
 * The SplitterOutput is used in the Splitter context to compose a output config
 * fpr the splitter construct.
 * 
 * @param <EIn>
 *            the entitiy which comes in the splitter
 * @param <EEx>
 *            the entity which leaves the splitter
 */
    public abstract class SplitterOutput<EIn, EEx> : HasSuccessor<EEx>
    where EIn : Entity
        where EEx : Entity
    {

        /**
         * Helper class for handeling the successor of this station
         */
        private SuccessorAdministration<EEx> successorAdministration;

        public void setSuccessor(Event eventToScheduleWhenFinished)
        {
            successorAdministration.setSuccessor(eventToScheduleWhenFinished);
        }

        public void setSuccessor(HasPredecessor<EEx> successor)
        {
            successorAdministration.setSuccessor(successor);
        }

        /**
         * the number of output parts of this output
         */
        private readonly int numberOfOutputPartsPerOriginalEntity;
        /**
         * the transport time of this output
         */
        private readonly NumericalDist<double> transportTime;

        /// <summary>
        /// Constructor to create a Splitteroutput which can be added to an List which than can passed to the
        /// Splitter
        /// </summary>
        /// <param name="numberOfOutputPartsPerOriginalEntity">
        /// the number of output parts of this output
        /// </param>
        /// <param name="transportTime">the transport time of this output</param>
        public SplitterOutput(Model owner, string name,
                int numberOfOutputPartsPerOriginalEntity,
                NumericalDist<double> transportTime)
        {
            this.numberOfOutputPartsPerOriginalEntity = numberOfOutputPartsPerOriginalEntity;
            this.transportTime = transportTime;

            successorAdministration = new SuccessorAdministration<EEx>(owner,
                    this.GetType().Name + ": " + name);
        }

        /// <summary>This Method has to be overridden by the user to set the output entity</summary>
        /// <param name="originalEntity">reference to the original entity</param>
        /// <returns>the output entitiy</returns>
        public abstract EEx createOutputEntity(EIn originalEntity);

        /// <summary>gets the number of parts</summary>
        /// <returns>the number of parts</returns>
        public int getNumberOfOutputPartsPerOriginalEntity()
        {
            return numberOfOutputPartsPerOriginalEntity;
        }

        /// <summary>gets the succsessor event which has been set by the user</summary>
        /// <returns>the succsessor event</returns>
        public Event getSuccessorEvent()
        {
            return successorAdministration.getSuccessorEvent();
        }

        /// <summary>gets the transport time of the station</summary>
        /// <returns>the transport time</returns>
        public TimeSpan getTransportTime()
        {
            if (transportTime == null)
            {
                return new TimeSpan(0);
            }
            else
            {
                return transportTime.sampleTimeSpan();
            }
        }

    }
}
