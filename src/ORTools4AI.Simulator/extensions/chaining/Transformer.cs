// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Christian Mentz
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.advancedModellingFeatures;
using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.engine;
using ORTools4AI.Simulator.extensions.chaining.abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ORTools4AI.Simulator.extensions.chaining
{
    /**
  * The Tranformer takes a given entitiy and transforms it into a new Type of
  * entity. The user has to override the method transformEntity() to define the
  * entitiy which the transformer should emit . The Tranformer inherits from the
  * Splitter and reduces its complexity for the new purpose. Within the
  * Transformer executes the same behaviour like the Splitter.
  * 
  * @param <EIn>
  *            the entitiy which comes in the Tranformer
  * @param <EEx>
  *            the entity which leaves the Transformer
  */
    public abstract class Transformer<EIn, EEx> : Splitter<EIn, EEx>, HasSuccessor<EEx>
        where EIn : Entity
        where EEx : Entity
    {
        private class TransformerSplitterOutput : SplitterOutput<EIn, EEx>
        {
            private readonly Transformer<EIn, EEx> _transformer;

            public TransformerSplitterOutput(Model owner, string name,
                NumericalDist<double> transportTime, Transformer<EIn, EEx> transformer)
                : base(owner, name, 1, transportTime)
            {
                _transformer = transformer;
            }

            public override EEx createOutputEntity(EIn originalEntity)
                => _transformer.transformEntity(originalEntity);
        }

        /// <summary>
        /// * This Constructor sets the given times, initializes the queues and fills the free service
        /// capacity queue. If a given int is lower than zero the Source assumes that there is no resriction.
        /// This applies to incomingBufferQueueCapacity, maxEntitiesToHandle, parallelHandledEntities
        /// </summary>
        /// <param name="incomingBufferQueueCapacity">the capatiy of the income queue</param>
        /// <param name="maxEntitiesToHandle">the number of max entities the station can handle</param>
        /// <param name="parallelHandledEntities">
        /// the number of max parallel entities a station can handle
        /// </param>
        /// <param name="setupTime">the time the station needs to set up</param>
        /// <param name="serviceTime">the time the station needs for the service</param>
        /// <param name="recoveryTime">the time the station needs tor recover</param>
        /// <param name="transportTime">
        /// the time the station needs to transport the entitiy to the next station
        /// </param>
        /// <param name="owner">the model owner</param>
        /// <param name="name">the name of the station</param>
        /// <param name="showInReport">should this construct be in the report</param>
        /// <param name="showInTrace">should this construct be in the trace</param>
        public Transformer(int incomingBufferQueueCapacity,
                int maxEntitiesToHandle, int parallelHandledEntities,
        NumericalDist<double> setupTime, NumericalDist<double> serviceTime,
                NumericalDist<double> recoveryTime, NumericalDist<double> transportTime,
                Model owner, string name, bool showInReport, bool showInTrace) : base(new List<SplitterOutput<EIn, EEx>>(),
                    incomingBufferQueueCapacity, maxEntitiesToHandle,
                    parallelHandledEntities, setupTime, serviceTime, recoveryTime,
                    owner, name, showInReport, showInTrace)
        {
            outputConfigurations.Add(new TransformerSplitterOutput(owner, name + "Output", transportTime, this));
        }

        public EEx createOutputEntity(EIn originalEntity)
        {
            return transformEntity(originalEntity);
        }

        public void setSuccessor(Event eventToScheduleWhenFinished)
        {
            outputConfigurations[0].setSuccessor(eventToScheduleWhenFinished);
        }

        public void setSuccessor(HasPredecessor<EEx> successor)
        {
            outputConfigurations[0].setSuccessor(successor);
        }

        /// <summary>
        /// This Method has to be overridden to create the output entity of the Transformer. The method gives
        /// the user the possibility to use the originalEntity in the creation of the transormEntity
        /// </summary>
        /// <param name="originalEntity">a reference to the original entity</param>
        /// <returns>the entity the Transformer sends to the next construct</returns>
        protected abstract EEx transformEntity(EIn originalEntity);

    }

}
